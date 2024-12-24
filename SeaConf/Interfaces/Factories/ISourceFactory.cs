using SeaConf.Interfaces.Core;

namespace SeaConf.Interfaces.Factories
{
    /// <summary>
    /// Factory that creates configuration data sources.
    /// </summary>
    public interface ISourceFactory
	{
        /// <summary>
        /// Create a configuration data source in storage.
        /// </summary>
        /// <returns>Configuration data source in storage.</returns>
		IStorageSource CreateStorageSource();

        /// <summary>
        /// Create a configuration data source in memory.
        /// </summary>
        /// <returns>Configuration data source in memory.</returns>
        IMemorySource CreateMemorySource();
    }
}