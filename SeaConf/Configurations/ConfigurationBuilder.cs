using SeaConf.Abstractions;
using SeaConf.Common;
using SeaConf.Core.Common.Abstractions.Factories;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;
using System.ComponentModel;
using SeaConf.Common.Enums;
using SeaConf.Common.Infrastructure;

namespace SeaConf.Configurations;

/// <summary>
/// Configuration builder.
/// </summary>
public class ConfigurationBuilder : IConfigurationBuilder
{
	private readonly Dictionary<ModelData, IModel> _models;
	private readonly Dictionary<Type, Type> _knownTypes;
	private readonly Dictionary<Type, IValueProviderFactory> _valueProviderFactories;
	private SyncMode _syncMode;
	private ISourceFactory? _sourceFactory;

	/// <summary>
	/// Getting a new builder.
	/// </summary>
	public static IConfigurationBuilder New => new ConfigurationBuilder();

	private ConfigurationBuilder()
	{
		_models = new Dictionary<ModelData, IModel>();
		_knownTypes = new Dictionary<Type, Type>();
		_syncMode = SyncMode.Disable;
		_valueProviderFactories = new Dictionary<Type, IValueProviderFactory>();
	}

	/// <inheritdoc />
	public IConfigurationBuilder WithSource(ISourceFactory sourceFactory)
	{
		_sourceFactory = Guard.ThrowIfNull(sourceFactory);
		return this;
	}

	/// <inheritdoc />
	public IConfigurationBuilder WithModel<T, TImpl>(string? name = null)
		where T : class
		where TImpl : PropertiesModel, T, new()
	{
		if (name != null && name.Trim().Length == 0)
		{
			throw new ArgumentException(nameof(name));
		}

		var type = typeof(T);
		var model = new TImpl();
		var key = new ModelData(IMemoryModel.GetName(type), type);

		if (!_models.TryAdd(key, model))
		{
			throw new InvalidOperationException(string.Format(Resources.ModelAlreadyAdded, key.Name, key.Type));
		}

		return this;
	}

	/// <inheritdoc />
	public IConfigurationBuilder WithKnownType<T, TImpl>()
		where T : class
		where TImpl : class, T
	{
		var key = typeof(T);

		if (_knownTypes.ContainsKey(key))
		{
			throw new InvalidOperationException(string.Format(Resources.TypeAlreadyAdded, key, typeof(TImpl)));
		}

		_knownTypes.Add(key, typeof(TImpl));

		return this;
	}

	/// <inheritdoc />
	public IConfigurationBuilder WithSyncMode(SyncMode syncMode)
	{
		_syncMode = Enum.IsDefined(syncMode)
			? syncMode
			: throw new InvalidEnumArgumentException(nameof(syncMode));

		return this;
	}

	/// <inheritdoc />
	public IConfigurationBuilder WithValueProviderFactory(IValueProviderFactory factory)
	{
		Guard.ThrowIfNull(factory);
		Guard.ThrowIfNull(factory.Type);

		_valueProviderFactories.Add(factory.Type, factory);

		return this;
	}

	/// <inheritdoc />
	public IConfiguration Build()
	{
		if (_sourceFactory == null)
		{
			throw new InvalidOperationException(Resources.ConfigurationSourceNotSet);
		}

		if (_models.Count == 0)
		{
			throw new InvalidOperationException(Resources.ModelsNotSet);
		}

		return new Configuration(_models, _knownTypes, _sourceFactory, _valueProviderFactories, _syncMode);
	}
}