using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppSettingsMini.Factories;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini
{
	public abstract class SettingsServiceBase : ISettingsService
	{
		private readonly ISettingsSourceProviderFactory _sourceProviderFactory;
		private readonly Dictionary<Type, ModelInfo> _models;
		private readonly Dictionary<Type, IValueProviderFactory> _valueProviderFactories;

		protected SettingsServiceBase(ISettingsSourceProviderFactory sourceProviderFactory)
		{
			Guard.ThrowIfNull(sourceProviderFactory, out _sourceProviderFactory);

			_models = new Dictionary<Type, ModelInfo>();
			_valueProviderFactories = new Dictionary<Type, IValueProviderFactory>();
		}

		protected void RegisterValueProviderFactory(IValueProviderFactory factory)
		{
			Guard.ThrowIfNull(factory);
			Guard.ThrowIfNull(factory.Type);

			if (_valueProviderFactories.ContainsKey(factory.Type))
			{
				throw new InvalidOperationException(string.Format(Strings.ProviderAlreadyRegistered, factory.Type.Name));
			}

			_valueProviderFactories.Add(factory.Type, factory);
		}

		protected TImpl RegisterModel<T, TImpl>(bool isReadOnly = false)
			where T : class
			where TImpl : SettingsModelBase, T, new()
		{
			var type = typeof(T);
			var model = new TImpl();

			if (_models.ContainsKey(type))
			{
				throw new InvalidOperationException(string.Format(Strings.ModelAlreadyRegistered, type.Name));
			}

			_models.Add(type, new ModelInfo(model, type.Name, isReadOnly));

			return model;
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
				using var valueProvidersFactory = new ValueProvidersFactory(_valueProviderFactories);

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
							throw new SettingsSaveLoadFaultException(string.Format(Strings.ProviderNotFound, property.Type.Name));
						}

						IPropertyData value;

						try
						{
							value = await provider.GetAsync(modelInfo.Name, property.Name).ConfigureAwait(false);
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
		}

		public async ValueTask SaveAsync()
		{
			if (_models.Count == 0)
			{
				throw new SettingsSaveLoadFaultException(Strings.NotFoundRegisteredModels);
			}

			var sourceProvider = _sourceProviderFactory.Create();

			await using (sourceProvider.ConfigureAwait(false))
			{
				using var valueProvidersFactory = new ValueProvidersFactory(_valueProviderFactories);

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
							throw new SettingsSaveLoadFaultException(string.Format(Strings.ProviderNotFound, property.Type.Name));
						}

						try
						{
							await provider.SetAsync(modelInfo.Name, property).ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							throw new SettingsSaveLoadFaultException(string.Format(Strings.FailedSavePropertyValue, property.Name, modelInfo.Name), ex);
						}
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
			public readonly bool IsReadOnly;

			public ModelInfo(ISettingsModel model, string name, bool isReadOnly)
			{
				Model = Guard.ThrowIfNullRet(model);
				Name = GetName(name);
				IsReadOnly = isReadOnly;
			}

			private static string GetName(string name)
			{
				Guard.ThrowIfEmptyString(name);

				if (name.Length == 1)
				{
					return name;
				}

				if (name.StartsWith("I"))
				{
					name = name.Substring(1, name.Length - 1);
				}

				return name;
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