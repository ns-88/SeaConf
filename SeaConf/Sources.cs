using SeaConf.Abstractions;
using SeaConf.Common;
using SeaConf.Core.Common.Abstractions.Factories;
using System.Reflection;
using System.Runtime.Versioning;

namespace SeaConf;

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
	/// <inheritdoc />
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
	/// <inheritdoc />
	public ISourceFactory Path(string appPath)
	{
		return new XmlSourceFactory(appPath);
	}

	/// <inheritdoc />
	public ISourceFactory LocalAppDataPath(string companyName, string appName)
	{
		var sep = System.IO.Path.DirectorySeparatorChar;

		var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{sep}{companyName}{sep}{appName}";

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		return new XmlSourceFactory($"{path}{sep}{Resources.SettingsFileName}.xml");
	}

	/// <inheritdoc />
	public ISourceFactory ExecutablePath()
	{
		var sep = System.IO.Path.DirectorySeparatorChar;
		var executingPath = Assembly.GetExecutingAssembly().Location;
		var directoryPath = System.IO.Path.GetDirectoryName(executingPath);

		return new XmlSourceFactory($"{directoryPath}{sep}{Resources.SettingsFileName}.xml");
	}
}