using System;
using System.Collections.Generic;
using System.ComponentModel;
using SeaConf.Core;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;
using SeaConf.Interfaces.Factories;
using SeaConf.Models;

namespace SeaConf
{
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

        /// <summary>
        /// Specifying the configuration data sources.
        /// </summary>
        /// <param name="sourceFactory">Configuration data sources factory.</param>
        /// <returns>Configuration builder.</returns>
        public IConfigurationBuilder WithSource(ISourceFactory sourceFactory)
        {
            _sourceFactory = Guard.ThrowIfNull(sourceFactory);
            return this;
        }

        /// <summary>
        /// Specifying the data model.
        /// </summary>
        /// <typeparam name="T">Model type (interface).</typeparam>
        /// <typeparam name="TImpl">Interface implementation.</typeparam>
        /// <param name="name">Name.</param>
        /// <returns>Configuration builder.</returns>
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
                throw new InvalidOperationException(string.Format(Strings.ModelAlreadyAdded, key.Name, key.Type));
            }

            return this;
        }

        /// <summary>
        /// Specifying the known type.
        /// </summary>
        /// <typeparam name="T">Type (interface).</typeparam>
        /// <typeparam name="TImpl">Interface implementation</typeparam>
        /// <returns>Configuration builder.</returns>
        public IConfigurationBuilder WithKnownType<T, TImpl>()
            where T : class
            where TImpl : class, T
        {
            var key = typeof(T);

            if (_knownTypes.ContainsKey(key))
            {
                throw new InvalidOperationException(string.Format(Strings.TypeAlreadyAdded, key, typeof(TImpl)));
            }

            _knownTypes.Add(key, typeof(TImpl));

            return this;
        }

        /// <summary>
        /// Specifying the sync mode.
        /// </summary>
        /// <param name="syncMode">Sync mode.</param>
        /// <returns>Configuration builder.</returns>
        public IConfigurationBuilder WithSyncMode(SyncMode syncMode)
        {
            _syncMode = Enum.IsDefined(syncMode)
                ? syncMode
                : throw new InvalidEnumArgumentException(nameof(syncMode));

            return this;
        }

        /// <summary>
        /// Specifying factory that creates provider for setting and getting data of a specific type.
        /// </summary>
        /// <param name="factory">Factory.</param>
        /// <returns>Configuration builder.</returns>
        public IConfigurationBuilder WithValueProviderFactory(IValueProviderFactory factory)
        {
            Guard.ThrowIfNull(factory);
            Guard.ThrowIfNull(factory.Type);

            _valueProviderFactories.Add(factory.Type, factory);

            return this;
        }

        /// <summary>
        /// Configuration build.
        /// </summary>
        /// <returns>Configuration.</returns>
        public IConfiguration Build()
        {
            if (_sourceFactory == null)
            {
                throw new InvalidOperationException(Strings.ConfigurationSourceNotSet);
            }

            if (_models.Count == 0)
            {
                throw new InvalidOperationException(Strings.ModelsNotSet);
            }

            return new Configuration(_models, _knownTypes, _sourceFactory, _valueProviderFactories, _syncMode);
        }
    }
}