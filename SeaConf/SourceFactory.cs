using SeaConf.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Factories;
using SeaConf.Core.Sources.MemorySource;
using SeaConf.Core.Sources.RegistrySource;
using SeaConf.Core.Sources.XmlSource;
using System.Runtime.Versioning;

namespace SeaConf;

/// <summary>
/// Factory that creates configuration data source in registry Windows.
/// </summary>
[SupportedOSPlatform("windows")]
internal class RegistrySourceFactory(string companyName, string appName) : ISourceFactory
{
	/// <inheritdoc />
	public IStorageSource CreateStorageSource()
	{
		return new RegistrySource(companyName, appName, Resources.RootCollectionName);
	}

	/// <inheritdoc />
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
	/// <inheritdoc />
	public IStorageSource CreateStorageSource()
	{
		return new XmlSource(path, Resources.RootCollectionName);
	}

	/// <inheritdoc />
	public IMemorySource CreateMemorySource()
	{
		return new MemorySource();
	}
}