using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Factories;

namespace SeaConf
{
    /// <summary>
    /// Standard data sources for configuration.
    /// </summary>
    public static class Sources
	{
        /// <summary>
        /// Builder of configuration data source in registry Windows.
        /// </summary>
        [SupportedOSPlatform("windows")]
		public static IRegistryPathBuilder Registry { get; } = new RegistryPathBuilder();

        /// <summary>
        /// Builder of configuration data source in xml-file.
        /// </summary>
        public static IXmlPathBuilder Xml { get; } = new XmlPathBuilder();
	}

    /// <summary>
    /// Builder of configuration data source in registry Windows.
    /// </summary>
    [SupportedOSPlatform("windows")]
	file class RegistryPathBuilder : IRegistryPathBuilder
	{
        /// <summary>
        /// Creating a configuration data source factory specifying the company name and application
        /// by the registry path "HKEY_CURRENT_USER\Software\companyName\appName\AppSettings".
        /// </summary>
        /// <param name="companyName">Company name.</param>
        /// <param name="appName">Application name.</param>
        /// <returns>Configuration data source factory.</returns>
        public ISourceFactory Path(string companyName, string appName)
		{
			return new RegistrySourceFactory(companyName, appName);
		}
	}

    /// <summary>
    /// Builder of configuration data source in xml-file.
    /// </summary>
    file class XmlPathBuilder : IXmlPathBuilder
	{
        /// <summary>
		/// Creating a factory of configuration data source specifying the path in the file system.
		/// </summary>
		/// <param name="appPath">Path.</param>
		/// <returns>Configuration data source factory.</returns>
		public ISourceFactory Path(string appPath)
		{
			return new XmlSourceFactory(appPath);
		}

        /// <summary>
        /// Creating a configuration data source factory specifying the company name and application
        /// by the local application data path.
        /// </summary>
        /// <param name="companyName">Company name.</param>
        /// <param name="appName">Application name.</param>
        /// <returns>Configuration data source factory.</returns>
        public ISourceFactory LocalAppDataPath(string companyName, string appName)
		{
			var sep = System.IO.Path.DirectorySeparatorChar;

			var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{sep}{companyName}{sep}{appName}";

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return new XmlSourceFactory($"{path}{sep}{Strings.SettingsFileName}.xml");
		}

        /// <summary>
        /// Creating a configuration data source factory for the current execution path.
        /// </summary>
        /// <returns>Configuration data source factory.</returns>
		public ISourceFactory ExecutablePath()
		{
			var sep = System.IO.Path.DirectorySeparatorChar;
			var executingPath = Assembly.GetExecutingAssembly().Location;
			var directoryPath = System.IO.Path.GetDirectoryName(executingPath);

			return new XmlSourceFactory($"{directoryPath}{sep}{Strings.SettingsFileName}.xml");
		}
	}
}