using System.Runtime.Versioning;
using SeaConf.Core.Sources;
using SeaConf.Infrastructure;
using SeaConf.Interfaces.Core;
using SeaConf.Interfaces.Factories;

namespace SeaConf
{
    /// <summary>
    /// Factory that creates configuration data source in registry Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
	internal class RegistrySourceFactory(string companyName, string appName) : ISourceFactory
	{
        /// <summary>
        /// Create a configuration data source in storage.
        /// </summary>
        /// <returns>Configuration data source in storage.</returns>
		public IStorageSource CreateStorageSource()
		{
			return new RegistrySource(companyName, appName, Strings.RootCollectionName);
		}

        /// <summary>
        /// Create a configuration data source in memory.
        /// </summary>
        /// <returns>Configuration data source in memory.</returns>
        public IMemorySource CreateMemorySource()
        {
            return new MemorySource();
        }
    }

    /// <summary>
    /// Factory that creates configuration data source in xml-file.
    /// </summary>
	internal class XmlSourceFactory(string path) : ISourceFactory
	{
        /// <summary>
        /// Create a configuration data source in storage.
        /// </summary>
        /// <returns>Configuration data source in storage.</returns>
		public IStorageSource CreateStorageSource()
        {
			return new XmlSource(path, Strings.RootCollectionName);
		}

        /// <summary>
        /// Create a configuration data source in memory.
        /// </summary>
        /// <returns>Configuration data source in memory.</returns>
        public IMemorySource CreateMemorySource()
        {
            return new MemorySource();
        }
    }
}