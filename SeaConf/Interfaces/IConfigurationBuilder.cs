using SeaConf.Interfaces.Factories;
using SeaConf.Models;

namespace SeaConf.Interfaces
{
    /// <summary>
    /// Configuration builder.
    /// </summary>
    public interface IConfigurationBuilder
    {
        /// <summary>
        /// Specifying the configuration data sources.
        /// </summary>
        /// <param name="sourceFactory">Configuration data sources factory.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder WithSource(ISourceFactory sourceFactory);

        /// <summary>
        /// Specifying the data model.
        /// </summary>
        /// <typeparam name="T">Model type (interface).</typeparam>
        /// <typeparam name="TImpl">Interface implementation.</typeparam>
        /// <param name="name">Name.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder WithModel<T, TImpl>(string? name = null)
            where T : class
            where TImpl : PropertiesModel, T, new();

        /// <summary>
        /// Specifying the known type.
        /// </summary>
        /// <typeparam name="T">Type (interface).</typeparam>
        /// <typeparam name="TImpl">Interface implementation</typeparam>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder WithKnownType<T, TImpl>()
            where T : class
            where TImpl : class, T;

        /// <summary>
        /// Specifying the sync mode.
        /// </summary>
        /// <param name="syncMode">Sync mode.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder WithSyncMode(SyncMode syncMode);

        /// <summary>
        /// Specifying factory that creates provider for setting and getting data of a specific type.
        /// </summary>
        /// <param name="factory">Factory.</param>
        /// <returns>Configuration builder.</returns>
        IConfigurationBuilder WithValueProviderFactory(IValueProviderFactory factory);

        /// <summary>
        /// Configuration build.
        /// </summary>
        /// <returns>Configuration.</returns>
        IConfiguration Build();
    }
}