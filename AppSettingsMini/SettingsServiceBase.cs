using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppSettingsMini.Core.Sources;
using AppSettingsMini.Core.ValueProviders;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.Models;

namespace AppSettingsMini
{
	public abstract class SettingsServiceBase : ISettingsService
	{
		private readonly ISourceFactory<IStorageModel> _sourceFactory;
		private readonly IReadOnlyDictionary<ModelData, IModelInfo> _models;
		private readonly ValueProvidersManager _valueProvidersManager;
		private readonly SynchronizationMode _synchronizationMode;

		internal ComparersManager ComparersManager => _valueProvidersManager.ComparersManager;

		public event EventHandler? Loaded;
		public event EventHandler<IChangedModels>? Saved;
		public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

		protected SettingsServiceBase(ISourceFactory<IStorageModel> sourceFactory, SynchronizationMode synchronizationMode = SynchronizationMode.Disable)
		{
			_sourceFactory = Guard.ThrowIfNull(sourceFactory);
			_synchronizationMode = synchronizationMode;

			_models = new Dictionary<ModelData, IModelInfo>();
			_valueProvidersManager = new ValueProvidersManager();
		}

		protected void Register(Action<RegisterContext> register)
		{
			if (_models.Count != 0)
			{
				throw new InvalidOperationException(Strings.ModelsForRegistrationNotFound);
			}

			var context = new RegisterContext(_valueProvidersManager);

			try
			{
				register(context);

				var models = (Dictionary<ModelData, IModelInfo>)_models;
				var source = new ModelInfoSource(context.Models, context.KnownTypes);
				var rootNodes = source.GetRootNodes().Result;
				var infoModels = source.GetModelsAsync(rootNodes).ToBlockingEnumerable();

				foreach (var modelInfo in infoModels)
				{
					var key = new ModelData(modelInfo.Name, modelInfo.Type);

					if (_models.ContainsKey(key))
					{
						throw new InvalidOperationException(string.Format(Strings.ModelAlreadyRegistered, key.Name, key.Type));
					}

					try
					{
						((IMemoryInitializedModel)modelInfo.Model).Initialize(modelInfo, this);
					}
					catch (Exception ex)
					{
						throw new InvalidOperationException(string.Format(Strings.ModelInitializationFailed, modelInfo.Type.Name), ex);
					}

					models.Add(key, modelInfo);
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(Strings.ModelRegistrationFailed, ex);
			}
		}

		internal void RaisePropertyChanged(string propertyName, IMemoryModel model)
		{
			Volatile.Read(ref PropertyChanged)?.Invoke(this, new PropertyChangedEventArgs(propertyName, model));
		}

		public async ValueTask LoadAsync()
		{
			if (_models.Count == 0)
			{
				throw new SaveLoadFaultException(Strings.NotFoundRegisteredModels);
			}

			// Создание общего источника данных, объединяющего источники в хранилище и памяти.
			var compositeSource = new CompositeSource(_sourceFactory.Create(), new MemorySource(_models.Values), _synchronizationMode);

			await using (compositeSource.ConfigureAwait(false))
			{
				// Создание фабрики провайдеров чтения и записи данных.
				using var valueProvidersFactory = _valueProvidersManager.CreateFactory();

				try
				{
					// Загрузка данных из источников.
					await compositeSource.LoadAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new SaveLoadFaultException(Strings.FailedLoadSettingsFromSource, ex);
				}

				// Получение итератора списка композитных моделей.
				var compositeModels = compositeSource.GetModelsAsync().ConfigureAwait(false);

				await foreach (var compositeModel in compositeModels.ConfigureAwait(false))
				{
					try
					{
						// Создание средства чтения данных из хранилища для текущей композитной модели.
						var reader = compositeModel.StorageModel.CreateReader();

						await using (reader.ConfigureAwait(false))
						{
							// Получение свойств из модели в памяти.
							var properties = compositeModel.MemoryModel.GetPropertiesData();

							foreach (var property in properties)
							{
								// Создание провайдера чтения и записи данных для конкретного типа.
								if (!valueProvidersFactory.TryCreate(property.Type, out var provider))
								{
									throw new SaveLoadFaultException(string.Format(Strings.ValueProviderNotFound, property.Type.Name));
								}

								IPropertyData value;

								try
								{
									// Получение значения для конкретного свойства из хранилища.
									value = await provider.GetAsync(reader, property).ConfigureAwait(false);
								}
								catch (Exception ex)
								{
									throw new SaveLoadFaultException(string.Format(Strings.FailedLoadPropertyValue, property.Name, compositeModel.MemoryModel.Name), ex);
								}

								try
								{
									// Присвоение полученного значения свойству.
									property.Set(value);
								}
								catch (Exception ex)
								{
									throw new SaveLoadFaultException(string.Format(Strings.FailedSetPropertyValue, property.Name, compositeModel.MemoryModel.Name), ex);
								}
							}
						}
					}
					finally
					{
						await compositeModel.DisposeAsync().ConfigureAwait(false);
					}
				}
			}

			Volatile.Read(ref Loaded)?.Invoke(this, EventArgs.Empty);
		}

		public async ValueTask SaveAsync()
		{
			if (_models.Count == 0)
			{
				throw new SaveLoadFaultException(Strings.NotFoundRegisteredModels);
			}

			var changedModels = new ChangedModels();

			// Создание общего источника данных, объединяющего источники в хранилище и памяти.
			var compositeSource = new CompositeSource(_sourceFactory.Create(), new MemorySource(_models.Values), _synchronizationMode);

			await using (compositeSource.ConfigureAwait(false))
			{
				// Создание фабрики провайдеров чтения и записи данных.
				using var valueProvidersFactory = _valueProvidersManager.CreateFactory();

				try
				{
					// Загрузка данных из источников.
					await compositeSource.LoadAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new SaveLoadFaultException(Strings.FailedLoadSettingsFromSource, ex);
				}

				// Получение итератора списка композитных моделей.
				var compositeModels = compositeSource.GetModelsAsync().ConfigureAwait(false);

				await foreach (var compositeModel in compositeModels.ConfigureAwait(false))
				{
					try
					{
						// Создание средства записи данных в хранилище для текущей композитной модели.
						var writer = compositeModel.StorageModel.CreateWriter();

						await using (writer.ConfigureAwait(false))
						{
							// Получение свойств, у которых были изменены данные из модели в памяти.
							var properties = compositeModel.MemoryModel.GetModifiedProperties();

							foreach (var property in properties)
							{
								// Создание провайдера чтения и записи данных для конкретного типа.
								if (!valueProvidersFactory.TryCreate(property.Type, out var provider))
								{
									throw new SaveLoadFaultException(string.Format(Strings.ValueProviderNotFound, property.Type.Name));
								}

								try
								{
									// Запись значения в хранилище.
									await provider.SetAsync(writer, property).ConfigureAwait(false);
								}
								catch (Exception ex)
								{
									throw new SaveLoadFaultException(string.Format(Strings.FailedSavePropertyValue, property.Name, compositeModel.MemoryModel.Name), ex);
								}

								changedModels.Add(compositeModel.MemoryModel.Type, property.Name);
							}
						}
					}
					finally
					{
						await compositeModel.DisposeAsync().ConfigureAwait(false);
					}
				}

				try
				{
					// Сохранение данных в источник.
					await compositeSource.SaveAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new SaveLoadFaultException(Strings.FailedSaveSettingsToSource, ex);
				}
			}

			Volatile.Read(ref Saved)?.Invoke(this, changedModels);
		}

		public T GetModel<T>(string? name = null)
		{
			if (name != null && name.Trim().Length == 0)
			{
				throw new ArgumentException(nameof(name));
			}

			var modelType = typeof(T);
			var modelName = name ?? IModelInfo.GetName(modelType);

			if (!_models.TryGetValue(new ModelData(modelName, modelType), out var modelInfo))
			{
				throw new InvalidOperationException(string.Format(Strings.ModelNotRegistered, modelType, modelName));
			}

			return (T)modelInfo.Model;
		}

		#region Nested types

		protected readonly struct RegisterContext
		{
			private readonly ValueProvidersManager _valueProvidersManager;
			private readonly Dictionary<ModelData, IModel> _models;
			private readonly Dictionary<Type, Type> _knownTypes;

			internal IReadOnlyDictionary<ModelData, IModel> Models => _models;
			internal IReadOnlyDictionary<Type, Type> KnownTypes => _knownTypes;

			internal RegisterContext(ValueProvidersManager valueProvidersManager)
			{
				_valueProvidersManager = valueProvidersManager;
				_models = new Dictionary<ModelData, IModel>();
				_knownTypes = new Dictionary<Type, Type>();
			}

			public void RegisterModel<T, TImpl>(string? name = null) where T : class where TImpl : ModelBase, T, new()
			{
				if (name != null && name.Trim().Length == 0)
				{
					throw new ArgumentException(nameof(name));
				}

				var type = typeof(T);
				var model = new TImpl();
				var key = new ModelData(IModelInfo.GetName(type), type);

				if (_models.ContainsKey(key))
				{
					throw new InvalidOperationException(string.Format(Strings.ModelAlreadyRegistered, key.Name, key.Type));
				}

				_models.Add(key, model);
			}

			public void RegisterValueProviderFactory(IValueProviderFactory factory)
			{
				_valueProvidersManager.AddFactory(factory);
			}

			public void RegisterType<T, TImpl>() where T : class where TImpl : class, T
			{
				var key = typeof(T);

				if (_knownTypes.ContainsKey(key))
				{
					throw new InvalidOperationException(string.Format(Strings.TypeAlreadyRegistered, key, typeof(TImpl)));
				}

				_knownTypes.Add(key, typeof(TImpl));
			}
		}

		#endregion
	}

	#region Infrastructure

	file class ModelInfoSource : SourceBase<IModelInfo>
	{
		private readonly IReadOnlyDictionary<ModelData, IModel> _models;
		private readonly IReadOnlyDictionary<Type, Type> _knownTypes;

		public ModelInfoSource(IReadOnlyDictionary<ModelData, IModel> models, IReadOnlyDictionary<Type, Type> knownTypes)
		{
			_models = models;
			_knownTypes = knownTypes;
		}

		public override ValueTask<IReadOnlyList<INode>> GetRootNodes()
		{
			var nodes = new List<INode>();

			foreach (var model in _models)
			{
				nodes.Add(new ModelInfoNode(model.Key.Name, model.Key.Type, model.Value, true, _knownTypes));
			}

			return ValueTask.FromResult((IReadOnlyList<INode>)nodes);
		}

		#region Nested types

		private class ModelInfoNode : INode, IModelInfo
		{
			private readonly List<IModelInfo> _innerModels;
			private readonly IReadOnlyDictionary<Type, Type> _knownTypes;

			public string Name { get; }
			public IMemoryModel Model { get; }
			public Type Type { get; }
			public bool IsRoot { get; }

			public IEnumerable<IModelInfo> InnerModels => _innerModels;

			public ModelInfoNode(string name, Type type, IModel model, bool isRoot, IReadOnlyDictionary<Type, Type> knownTypes)
			{
				_innerModels = new List<IModelInfo>();
				_knownTypes = knownTypes;

				Model = (IMemoryModel)model;
				Name = name;
				Type = type;
				IsRoot = isRoot;
			}

			private void CreateAndSetInnerModel(PropertyInfo property)
			{
				object? innerModel;

				if (!_knownTypes.TryGetValue(property.PropertyType, out var activatedType))
				{
					activatedType = property.PropertyType;
				}

				try
				{
					innerModel = Activator.CreateInstance(activatedType);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(string.Format(Strings.FailedCreateNestedModelInstance, IModelInfo.GetName(property.PropertyType)), ex);
				}

				try
				{
					if (property.CanWrite)
					{
						property.SetValue(Model, innerModel);
					}
					else
					{
						var backingFieldName = $"<{property.Name}>k__BackingField";
						var backingField = Model.GetType().GetField(backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic);

						if (backingField != null)
						{
							backingField.SetValue(Model, innerModel);
						}
						else
						{
							throw new InvalidOperationException(string.Format(Strings.FailedGetBackingField, backingFieldName, property.Name));
						}
					}
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(string.Format(Strings.FailedSetPropertyValue, property.Name, Name), ex);
				}
			}

			public ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
			{
				var nodes = new List<INode>();
				var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

				foreach (var property in properties)
				{
					var attribute = property.GetCustomAttribute<ModelAttribute>();

					if (attribute == null)
					{
						continue;
					}

					var innerModelRaw = property.GetValue(Model);

					if (innerModelRaw == null)
					{
						CreateAndSetInnerModel(property);
					}

					if (innerModelRaw is not IMemoryModel innerModel)
					{
						throw new InvalidOperationException(
							string.Format(Strings.InvalidNestedModelType, typeof(ModelAttribute), typeof(IModelInfo), property.PropertyType, Type));
					}

					var innerModelName = IModelInfo.GetName(property.PropertyType, attribute);
					var node = new ModelInfoNode(innerModelName, property.PropertyType, innerModel, false, _knownTypes);

					_innerModels.Add(node);
					nodes.Add(node);
				}

				return ValueTask.FromResult((IReadOnlyList<INode>)nodes);
			}

			public override string ToString()
			{
				return $"Name = {Name}, Type = {Type.Name}, InnerModels = {InnerModels.Count()}";
			}
		}

		#endregion
	}

	file readonly struct CompositeSource : IAsyncDisposable
	{
		private readonly ISource<IStorageModel> _storageSource;
		private readonly ISource<IMemoryModel> _memorySource;
		private readonly SynchronizationMode _synchronizationMode;

		public CompositeSource(ISource<IStorageModel> storageSource, ISource<IMemoryModel> memorySource, SynchronizationMode synchronizationMode)
		{
			_storageSource = storageSource;
			_memorySource = memorySource;
			_synchronizationMode = synchronizationMode;
		}

		private async Task<Models> GetAllModelsFromSourcesAsync(IReadOnlyList<SynchronizedNodes> synchronizedNodes)
		{
			var memoryModels = _memorySource.GetModelsAsync(synchronizedNodes.Select(x => x.MemoryNode)).ConfigureAwait(false);
			var storageModels = _storageSource.GetModelsAsync(synchronizedNodes.Select(x => x.StorageNode)).ConfigureAwait(false);

			var memoryModelsMap = new Dictionary<ModelPath, IMemoryModel>();
			var storageModelsMap = new Dictionary<ModelPath, IStorageModel>();

			var memoryModelsMapFillTask = Task.Run(async () =>
			{
				await foreach (var memoryModel in memoryModels.ConfigureAwait(false))
				{
					memoryModelsMap.Add(((IPathModel)memoryModel).Path, memoryModel);
				}
			});

			var storageModelsMapFillTask = Task.Run(async () =>
			{
				await foreach (var storageModel in storageModels.ConfigureAwait(false))
				{
					storageModelsMap.Add(((IPathModel)storageModel).Path, storageModel);
				}
			});

			try
			{
				await Task.WhenAll(memoryModelsMapFillTask, storageModelsMapFillTask).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(Strings.GetModelsFromMemoryAndStorageFailed, ex);
			}

			return new Models(memoryModelsMap, storageModelsMap);
		}

		private async IAsyncEnumerable<CompositeModel> SynchronizationAsync(IReadOnlyCollection<INode> rootMemoryNodes, IReadOnlyCollection<INode> rootStorageNodes)
		{
			if (rootMemoryNodes.Count == 0)
			{
				throw new InvalidOperationException(Strings.ViolationStorageStructureNoModelsInStorage);
			}

			// ~~~Синхронизируем корневые модели~~~.

			var nodeFound = false;
			var synchronizedNodes = new List<SynchronizedNodes>();

			// Добавление моделей.
			foreach (var rootMemoryNode in rootMemoryNodes)
			{
				foreach (var rootStorageNode in rootStorageNodes)
				{
					if (!rootMemoryNode.Name.Equals(rootStorageNode.Name, StringComparison.Ordinal))
					{
						continue;
					}

					synchronizedNodes.Add(new SynchronizedNodes(rootMemoryNode, rootStorageNode));

					nodeFound = true;
					break;
				}

				if (nodeFound)
				{
					nodeFound = false;
					continue;
				}

				INode rootStorageNodeNew;
				var path = ((IPathModel)rootMemoryNode).Path;

				try
				{
					rootStorageNodeNew = (INode)((IWritableSource<IStorageModel>)_storageSource).AddModel(path);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(string.Format(Strings.AddingNewRootModelFailed, path, rootMemoryNode.Name), ex);
				}

				synchronizedNodes.Add(new SynchronizedNodes(rootMemoryNode, rootStorageNodeNew));
			}

			nodeFound = false;

			// Удаление моделей.
			foreach (var rootStorageNode in rootStorageNodes)
			{
				foreach (var node in synchronizedNodes)
				{
					if (!rootStorageNode.Name.Equals(node.StorageNode.Name, StringComparison.Ordinal))
					{
						continue;
					}

					nodeFound = true;
					break;
				}

				if (nodeFound)
				{
					nodeFound = false;
					continue;
				}

				var path = ((IPathModel)rootStorageNode).Path;

				try
				{
					((IWritableSource<IStorageModel>)_storageSource).DeleteModel(path);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(string.Format(Strings.DeletingRootModelFailed, path, rootStorageNode.Name), ex);
				}
			}

			// ~~~Синхронизируем дочерние модели~~~.

			// Получаем все модели из источника в памяти и хранилище.
			var models = await GetAllModelsFromSourcesAsync(synchronizedNodes).ConfigureAwait(false);

			// Удаление моделей.
			foreach (var storageModel in models.Storage)
			{
				if (models.Memory.ContainsKey(storageModel.Key))
				{
					continue;
				}

				try
				{
					var nodes = await ((INode)storageModel.Value).GetDescendantNodesAsync().ConfigureAwait(false);

					foreach (var node in nodes)
					{
						models.Storage.Remove(((IPathModel)node).Path);
					}

					models.Storage.Remove(storageModel.Key);

					((IWritableSource<IStorageModel>)_storageSource).DeleteModel(storageModel.Key);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException(string.Format(Strings.DeletingModelFailed, storageModel.Key, storageModel.Value.Name), ex);
				}
			}

			// Добавление моделей.
			foreach (var (path, model) in models.Memory)
			{
				if (!models.Storage.TryGetValue(path, out var storageModel))
				{
					try
					{
						storageModel = ((IWritableSource<IStorageModel>)_storageSource).AddModel(path);
					}
					catch (Exception ex)
					{
						throw new InvalidOperationException(string.Format(Strings.AddingNewModelFailed, path, model.Name), ex);
					}
				}

				yield return new CompositeModel(model, storageModel);
			}
		}

		public async IAsyncEnumerable<CompositeModel> GetModelsAsync()
		{
			var rootMemoryNodes = await _memorySource.GetRootNodes().ConfigureAwait(false);
			var rootStorageNodes = await _storageSource.GetRootNodes().ConfigureAwait(false);

			if (_synchronizationMode == SynchronizationMode.Enable || (_synchronizationMode == SynchronizationMode.EnableIfDebug && Debugger.IsAttached))
			{
				var compositeModels = SynchronizationAsync(rootMemoryNodes, rootStorageNodes).ConfigureAwait(false);

				await foreach (var model in compositeModels.ConfigureAwait(false))
				{
					yield return model;
				}
			}
			else
			{
				if (rootMemoryNodes.Count == 0)
				{
					throw new InvalidOperationException(Strings.ViolationStorageStructureNoModelsInMemory);
				}

				if (rootStorageNodes.Count == 0)
				{
					throw new InvalidOperationException(Strings.ViolationStorageStructureNoModelsInStorage);
				}

				if (rootMemoryNodes.Count != rootStorageNodes.Count)
				{
					throw new InvalidOperationException(
						string.Format(Strings.ViolationStorageStructureRootModelsNumberDoesNotMatch, rootMemoryNodes.Count, rootStorageNodes.Count));
				}

				// ~~~Синхронизируем корневые модели~~~.

				var nodeFound = false;
				var synchronizedNodes = new List<SynchronizedNodes>();

				foreach (var memoryNode in rootMemoryNodes)
				{
					foreach (var storageNode in rootStorageNodes)
					{
						if (!memoryNode.Name.Equals(storageNode.Name, StringComparison.Ordinal))
						{
							continue;
						}

						synchronizedNodes.Add(new SynchronizedNodes(memoryNode, storageNode));
						nodeFound = true;

						break;
					}

					if (!nodeFound)
					{
						throw new InvalidOperationException(
							string.Format(Strings.ViolationStorageStructureModelFromStorageNotFound, ((IPathModel)memoryNode).Path, memoryNode.Name));
					}

					nodeFound = false;
				}

				// Получаем все модели из источника в памяти и хранилище.
				var models = await GetAllModelsFromSourcesAsync(synchronizedNodes).ConfigureAwait(false);

				if (models.Memory.Count != models.Storage.Count)
				{
					throw new InvalidOperationException(
						string.Format(Strings.ViolationStorageStructureModelsNumberDoesNotMatch, models.Memory.Count, models.Storage.Count));
				}

				foreach (var (path, memoryModel) in models.Memory)
				{
					if (!models.Storage.TryGetValue(path, out var storageModel))
					{
						throw new InvalidOperationException(
							string.Format(Strings.ViolationStorageStructureModelFromStorageNotFound, path, memoryModel.Name));
					}

					yield return new CompositeModel(memoryModel, storageModel);
				}
			}
		}

		public async ValueTask LoadAsync()
		{
			await _storageSource.LoadAsync().ConfigureAwait(false);
			await _memorySource.LoadAsync().ConfigureAwait(false);
		}

		public async ValueTask SaveAsync()
		{
			await _storageSource.SaveAsync().ConfigureAwait(false);
			await _memorySource.SaveAsync().ConfigureAwait(false);
		}

		public async ValueTask DisposeAsync()
		{
			if (_storageSource != null)
			{
				await _storageSource.DisposeAsync().ConfigureAwait(false);
			}

			if (_memorySource != null)
			{
				await _memorySource.DisposeAsync().ConfigureAwait(false);
			}
		}

		private readonly record struct SynchronizedNodes(INode MemoryNode, INode StorageNode);

		private readonly record struct Models(IReadOnlyDictionary<ModelPath, IMemoryModel> Memory, Dictionary<ModelPath, IStorageModel> Storage);
	}

	internal readonly struct ModelData : IEquatable<ModelData>
	{
		public readonly Type Type;
		public readonly string Name;

		public ModelData(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		public bool Equals(ModelData other)
		{
			return Name.Equals(other.Name, StringComparison.Ordinal) && Type == other.Type;
		}

		public override bool Equals(object? obj)
		{
			return obj is ModelData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name, Type);
		}

		public override string ToString()
		{
			return Name;
		}
	}

	#endregion
}