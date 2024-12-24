using SeaConf.Interfaces.Factories;

namespace SeaConf.Interfaces
{
    /// <summary>
    /// Builder of configuration data source in registry Windows.
    /// </summary>
    public interface IRegistryPathBuilder
	{
        /// <summary>
        /// Creating a configuration data source factory specifying the company name and application
        /// by the registry path "HKEY_CURRENT_USER\Software\companyName\appName\AppSettings".
        /// </summary>
        /// <param name="companyName">Company name.</param>
        /// <param name="appName">Application name.</param>
        /// <returns>Configuration data source factory.</returns>
		ISourceFactory Path(string companyName, string appName);
	}

    /// <summary>
    /// Builder of configuration data source in xml-file.
    /// </summary>
    public interface IXmlPathBuilder
	{
        /// <summary>
        /// Creating a factory of configuration data source specifying the path in the file system.
        /// </summary>
        /// <param name="appPath">Path.</param>
        /// <returns>Configuration data source factory.</returns>
        ISourceFactory Path(string appPath);

        /// <summary>
        /// Creating a configuration data source factory specifying the company name and application
        /// by the local application data path.
        /// </summary>
        /// <param name="companyName">Company name.</param>
        /// <param name="appName">Application name.</param>
        /// <returns>Configuration data source factory.</returns>
		ISourceFactory LocalAppDataPath(string companyName, string appName);

        /// <summary>
        /// Creating a configuration data source factory for the current execution path.
        /// </summary>
        /// <returns>Configuration data source factory.</returns>
		ISourceFactory ExecutablePath();
	}
}