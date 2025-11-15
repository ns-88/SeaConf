using SeaConf.Abstractions;
using SeaConf.Common;
using SeaConf.Common.Enums;
using SeaConf.Common.Exceptions;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Factories;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;
using SeaConf.Core.Common.ValueProviders;

namespace SeaConf.Configurations;

/// <inheritdoc cref="IConfiguration" />
internal partial class Configuration : IConfiguration
{
	private readonly ISourceFactory _sourceFactory;
	private readonly IMemorySource _memorySource;
	private readonly ValueProvidersManager _valueProvidersManager;
	private readonly SyncMode _syncMode;

	/// <inheritdoc cref="IConfiguration" />
	public IReadOnlyDictionary<ModelData, IModel> RegisteredModels { get; }

	/// <inheritdoc cref="IConfiguration" />
	public IReadOnlyDictionary<Type, Type> KnownTypes { get; }

	/// <inheritdoc />
	public event EventHandler? Loaded;

	/// <inheritdoc />
	public event EventHandler<ChangedModelsEventArgs>? Saved;

	/// <inheritdoc />
	public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

	public Configuration(
		IReadOnlyDictionary<ModelData, IModel> registeredModels,
		IReadOnlyDictionary<Type, Type> knownTypes,
		ISourceFactory sourceFactory,
		IReadOnlyDictionary<Type, IValueProviderFactory> valueProviderFactories,
		SyncMode syncMode = SyncMode.Disable
	)
	{
		_memorySource = sourceFactory.CreateMemorySource();
		_valueProvidersManager = new ValueProvidersManager(valueProviderFactories);
		_sourceFactory = sourceFactory;
		_syncMode = syncMode;

		RegisteredModels = registeredModels;
		KnownTypes = knownTypes;

		Initialize();
	}

	private void Initialize()
	{
		try
		{
			if (_memorySource == null!)
			{
				throw new InvalidOperationException(Resources.MemorySourceNotCreated);
			}

			_memorySource.Initialize(this);
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(Resources.MemorySourceInitializationFailed, ex);
		}
	}

	/// <inheritdoc />
	public async ValueTask LoadAsync()
	{
		try
		{
			// Создание фабрики провайдеров чтения и записи данных.
			using var valueProvidersFactory = _valueProvidersManager.CreateFactory();

			// Создание источника конфигурации в хранилище.
			var storageSource = _sourceFactory.CreateStorageSource();

			await using (storageSource.ConfigureAwait(false))
			{
				// Создание средства чтения конфигурации.
				var reader = new ConfigurationReader(_memorySource, storageSource, valueProvidersFactory, this, _syncMode);

				// Выполнение чтения конфигурации.
				await reader.ReadAsync().ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			throw new ConfigurationLoadFaultException(Resources.ConfigurationLoadFailed, ex);
		}
	}

	/// <inheritdoc />
	public async ValueTask SaveAsync()
	{
		try
		{
			// Создание фабрики провайдеров чтения и записи данных.
			using var valueProvidersFactory = _valueProvidersManager.CreateFactory();

			// Создание источника конфигурации в хранилище.
			var storageSource = _sourceFactory.CreateStorageSource();

			await using (storageSource.ConfigureAwait(false))
			{
				// Создание средства записи конфигурации.
				var writer = new ConfigurationWriter(_memorySource, storageSource, valueProvidersFactory, this, _syncMode);

				// Выполнение записи конфигурации.
				await writer.WriteAsync().ConfigureAwait(false);
			}
		}
		catch (Exception ex)
		{
			throw new ConfigurationSaveFaultException(Resources.ConfigurationSaveFailed, ex);
		}
	}

	/// <inheritdoc />
	public T GetModel<T>(string? name = null) where T : class
	{
		return _memorySource.GetModel<T>(name);
	}
}