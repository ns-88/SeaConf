using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.Models;

namespace AppSettingsMini
{
	public abstract class SettingsServiceBase : ISettingsService
	{
		private readonly ISettingsSourceProviderFactory _sourceProviderFactory;
		private readonly Dictionary<Type, ModelInfo> _models;
		private readonly ValueProvidersManager _valueProvidersManager;

		internal ComparersManager ComparersManager => _valueProvidersManager.ComparersManager;

		public event EventHandler? Loaded;
		public event EventHandler<IChangedModels>? Saved;
		public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

		protected SettingsServiceBase(ISettingsSourceProviderFactory sourceProviderFactory)
		{
			Guard.ThrowIfNull(sourceProviderFactory, out _sourceProviderFactory);

			_models = new Dictionary<Type, ModelInfo>();
			_valueProvidersManager = new ValueProvidersManager();
		}

		protected void RegisterValueProviderFactory(IValueProviderFactory factory)
		{
			_valueProvidersManager.AddFactory(factory);
		}

		protected TImpl RegisterModel<T, TImpl>(bool isReadOnly = false, string? modelName = null)
			where T : class
			where TImpl : SettingsModelBase, T, new()
		{
			if (modelName == string.Empty || modelName?.Trim().Length == 0)
			{
				throw new ArgumentException(nameof(modelName));
			}

			var type = typeof(T);
			var model = new TImpl();

			if (_models.ContainsKey(type))
			{
				throw new InvalidOperationException(string.Format(Strings.ModelAlreadyRegistered, type.Name));
			}

			try
			{
				((ISettingsModel)model).Init(this);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(Strings.ModelInitializationFailed, type.Name), ex);
			}

			_models.Add(type, new ModelInfo(model, type, isReadOnly, modelName));

			return model;
		}

		internal void RaisePropertyChanged(string propertyName, ISettingsModel model)
		{
			Volatile.Read(ref PropertyChanged)?.Invoke(this, new PropertyChangedEventArgs(propertyName, model));
		}

		public async ValueTask LoadAsync()
		{
			if (_models.Count == 0)
			{
				throw new SettingsSaveLoadFaultException(Strings.NotFoundRegisteredModels);
			}

			var sourceProvider = _sourceProviderFactory.Create();

			await using (sourceProvider.ConfigureAwait(false))
			{
				using var valueProvidersFactory = _valueProvidersManager.CreateFactory();

				try
				{
					await sourceProvider.LoadAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new SettingsSaveLoadFaultException(Strings.FailedLoadSettingsFromSource, ex);
				}

				foreach (var modelInfo in _models.Values)
				{
					var properties = modelInfo.Model.GetPropertiesData().Values;

					foreach (var property in properties)
					{
						if (!valueProvidersFactory.TryCreate(property.Type, sourceProvider, out var provider))
						{
							throw new SettingsSaveLoadFaultException(string.Format(Strings.ValueProviderNotFound, property.Type.Name));
						}

						ISettingsPropertyData value;

						try
						{
							value = await provider.GetAsync(modelInfo.Name, property).ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							throw new SettingsSaveLoadFaultException(string.Format(Strings.FailedLoadPropertyValue, property.Name, modelInfo.Name), ex);
						}

						try
						{
							property.Set(value);
						}
						catch (Exception ex)
						{
							throw new SettingsSaveLoadFaultException(string.Format(Strings.FailedSetPropertyValue, property.Name, modelInfo.Name), ex);
						}
					}
				}
			}

			Volatile.Read(ref Loaded)?.Invoke(this, EventArgs.Empty);
		}

		public async ValueTask SaveAsync()
		{
			if (_models.Count == 0)
			{
				throw new SettingsSaveLoadFaultException(Strings.NotFoundRegisteredModels);
			}

			var changedModels = new ChangedModels();
			var sourceProvider = _sourceProviderFactory.Create();

			await using (sourceProvider.ConfigureAwait(false))
			{
				using var valueProvidersFactory = _valueProvidersManager.CreateFactory();

				try
				{
					await sourceProvider.LoadAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new SettingsSaveLoadFaultException(Strings.FailedLoadSettingsFromSource, ex);
				}

				foreach (var modelInfo in _models.Values)
				{
					if (modelInfo.IsReadOnly)
					{
						continue;
					}

					var properties = modelInfo.Model.GetModifiedProperties();

					foreach (var property in properties)
					{
						if (!valueProvidersFactory.TryCreate(property.Type, sourceProvider, out var provider))
						{
							throw new SettingsSaveLoadFaultException(string.Format(Strings.ValueProviderNotFound, property.Type.Name));
						}

						try
						{
							await provider.SetAsync(modelInfo.Name, property).ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							throw new SettingsSaveLoadFaultException(string.Format(Strings.FailedSavePropertyValue, property.Name, modelInfo.Name), ex);
						}

						changedModels.Add(modelInfo.Type, property.Name);
					}
				}

				try
				{
					await sourceProvider.SaveAsync().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					throw new SettingsSaveLoadFaultException(Strings.FailedSaveSettingsToSource, ex);
				}
			}

			Volatile.Read(ref Saved)?.Invoke(this, changedModels);
		}

		public T GetModel<T>()
		{
			var type = typeof(T);

			if (!_models.TryGetValue(type, out var modelInfo))
			{
				throw new InvalidOperationException(string.Format(Strings.ModelNotRegistered, type));
			}

			return (T)modelInfo.Model;
		}

		#region Nested types

		private readonly struct ModelInfo
		{
			public readonly ISettingsModel Model;
			public readonly string Name;
			public readonly Type Type;
			public readonly bool IsReadOnly;

			public ModelInfo(ISettingsModel model, Type type, bool isReadOnly, string? modelName)
			{
				Model = model;
				Name = GetName(type, modelName);
				Type = type;
				IsReadOnly = isReadOnly;
			}

			private static string GetName(MemberInfo memberInfo, string? modelName)
			{
				if (modelName != null)
				{
					return modelName;
				}

				var typeName = memberInfo.Name;

				if (typeName.Length == 1)
				{
					return typeName;
				}

				if (typeName.StartsWith("I"))
				{
					typeName = typeName.Substring(1, typeName.Length - 1);
				}

				return typeName;
			}

			public override bool Equals(object? obj)
			{
				if (obj is not ModelInfo other)
				{
					return false;
				}

				return Model == other.Model &&
					   Name.Equals(other.Name, StringComparison.Ordinal) &&
					   IsReadOnly == other.IsReadOnly;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Model, Name, IsReadOnly);
			}
		}

		#endregion
	}
}