using System.Runtime.Versioning;
using AppSettingsMini.Core.Sources;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini
{
	[SupportedOSPlatform("windows")]
	internal class RegistrySourceFactory(string companyName, string appName) : ISourceFactory<IStorageModel>
	{
		public ISource<IStorageModel> Create()
		{
			return new RegistrySource(companyName, appName, Strings.RootCollectionName);
		}
	}

	internal class XmlSourceFactory(string path) : ISourceFactory<IStorageModel>
	{
		public ISource<IStorageModel> Create()
		{
			return new XmlSource(path, Strings.RootCollectionName);
		}
	}
}