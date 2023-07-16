using System.Runtime.Versioning;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.SettingsSources.RegistrySource;
using AppSettingsMini.SettingsSources.XmlSource;

namespace AppSettingsMini
{
	public static class Sources
	{
		private const string RootCollectionName = "AppSettings";

#if NET6_0
		[SupportedOSPlatform("windows")]
#endif
		public static ISettingsSourceProviderFactory FromRegistry(string appPath)
		{
			return new RegistryProviderFactory(appPath, RootCollectionName);
		}

		public static ISettingsSourceProviderFactory FromXml(string path)
		{
			return new XmlProviderFactory(path, RootCollectionName);
		}

		#region Nested types

#if NET6_0
		[SupportedOSPlatform("windows")]
#endif
		private class RegistryProviderFactory : ISettingsSourceProviderFactory
		{
			private readonly string _appPath;
			private readonly string _rootKeyName;

			public RegistryProviderFactory(string appPath, string rootKeyName)
			{
				_appPath = appPath;
				_rootKeyName = rootKeyName;
			}

			public ISettingsSourceProvider Create()
			{
				return new RegistrySettingsSourcesProvider(_appPath, _rootKeyName);
			}
		}

		private class XmlProviderFactory : ISettingsSourceProviderFactory
		{
			private readonly string _path;
			private readonly string _rootElementName;

			public XmlProviderFactory(string path, string rootElementName)
			{
				_path = path;
				_rootElementName = rootElementName;
			}

			public ISettingsSourceProvider Create()
			{
				return new XmlSettingsSourcesProvider(_path, _rootElementName);
			}
		}

		#endregion
	}
}