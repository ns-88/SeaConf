using Settings.Infrastructure;
using Settings.Interfaces;
using Settings.Interfaces.ValueProviders;
using Settings.ValueProviders;

namespace Settings
{
	public abstract class SettingsServiceBase
	{
		private const string DefaultCollectionName = "AppSettings";

		private readonly Dictionary<Type, ModelInfo> _models;
		private readonly Dictionary<Type, IValueProvider> _valueProviders;

		protected SettingsServiceBase(ISettingsSourceProvider sourceProvider, string? collectionName = null)
		{
			Guard.ThrowIfNull(sourceProvider);

			_models = new Dictionary<Type, ModelInfo>();
			_valueProviders = CreateDefaultProviders(collectionName, sourceProvider);
		}

		private static Dictionary<Type, IValueProvider> CreateDefaultProviders(string? collectionName, ISettingsSourceProvider sourceProvider)
		{
			if (string.IsNullOrWhiteSpace(collectionName))
			{
				collectionName = DefaultCollectionName;
			}

			return new Dictionary<Type, IValueProvider>
			{
				{ typeof(string), new StringValueProvider(collectionName, sourceProvider) },
				{ typeof(int), new IntValueProvider(collectionName, sourceProvider) },
				{ typeof(long), new LongValueProvider(collectionName, sourceProvider) },
				{ typeof(double), new DoubleValueProvider(collectionName, sourceProvider) },
				{ typeof(ReadOnlyMemory<byte>), new BytesValueProvider(collectionName, sourceProvider) },
			};
		}

		protected void RegisterValueProvider(IValueProvider provider)
		{
			Guard.ThrowIfNull(provider);
			Guard.ThrowIfNull(provider.Type);

			if (_valueProviders.ContainsKey(provider.Type))
			{
				throw new InvalidOperationException(string.Format(Strings.ProviderAlreadyRegistered, provider.Type.Name));
			}

			_valueProviders.Add(provider.Type, provider);
		}

		protected T RegisterModel<T>(string name, bool isReadOnly) where T : SettingsModelBase, new()
		{
			Guard.ThrowIfEmptyString(name);

			var type = typeof(T);
			var model = new T();

			if (_models.ContainsKey(type))
			{
				throw new InvalidOperationException(string.Format(Strings.ModelAlreadyRegistered, type.Name));
			}

			_models.Add(type, new ModelInfo(model, name, isReadOnly));

			return model;
		}

		public async ValueTask LoadAsync()
		{
			if (_models.Count == 0)
			{
				throw new SettingsSaveLoadFaultException(Strings.NotFoundRegisteredModels);
			}

			foreach (var modelInfo in _models.Values)
			{
				var properties = modelInfo.Model.GetPropertiesData().Values;

				foreach (var property in properties)
				{
					if (!_valueProviders.TryGetValue(property.Type, out var provider))
					{
						throw new SettingsSaveLoadFaultException(string.Format(Strings.ProviderNotFound, property.Type.Name));
					}

					IPropertyData value;

					try
					{
						value = await provider.GetAsync(property.Name).ConfigureAwait(false);
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

		public async ValueTask SaveAsync()
		{
			if (_models.Count == 0)
			{
				throw new SettingsSaveLoadFaultException(Strings.NotFoundRegisteredModels);
			}

			foreach (var modelInfo in _models)
			{
				if (modelInfo.Value.IsReadOnly)
				{
					continue;
				}

				var properties = modelInfo.Value.Model.GetModifiedProperties();

				foreach (var property in properties)
				{
					if (_valueProviders.TryGetValue(property.Type, out var provider))
					{
						try
						{
							await provider.SetAsync(property).ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							throw new SettingsSaveLoadFaultException(string.Format(Strings.FailedSavePropertyValue, property.Name, modelInfo.Value.Name), ex);
						}
					}
					else
					{
						throw new SettingsSaveLoadFaultException(string.Format(Strings.ProviderNotFound, property.Type.Name));
					}
				}
			}
		}

		#region Nested types

		private readonly struct ModelInfo
		{
			public readonly ISettingsModel Model;
			public readonly string Name;
			public readonly bool IsReadOnly;

			public ModelInfo(ISettingsModel model, string name, bool isReadOnly)
			{
				Model = model;
				Name = name;
				IsReadOnly = isReadOnly;
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