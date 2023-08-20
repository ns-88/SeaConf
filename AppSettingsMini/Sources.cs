using System;
using System.IO;
using System.Reflection;
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
		private const string SettingsFileName = "Settings";

#if NET6_0
		[SupportedOSPlatform("windows")]
#endif
		public static IRegistryPathBuilder Registry { get; } = new RegistryPathBuilder();

		public static IXmlPathBuilder Xml { get; } = new XmlPathBuilder();

		#region Nested types
#if NET6_0
		[SupportedOSPlatform("windows")]
#endif
		private class RegistryProviderFactory : ISettingsSourceProviderFactory
		{
			private readonly string _companyName;
			private readonly string _appName;

			public RegistryProviderFactory(string companyName, string appName)
			{
				_companyName = companyName;
				_appName = appName;
			}

			public ISettingsSourceProvider Create()
			{
				return new RegistrySettingsSourcesProvider(_companyName, _appName, RootCollectionName);
			}
		}

		private class XmlProviderFactory : ISettingsSourceProviderFactory
		{
			private readonly string _path;

			public XmlProviderFactory(string path)
			{
				_path = path;
			}

			public ISettingsSourceProvider Create()
			{
				return new XmlSettingsSourcesProvider(_path, RootCollectionName);
			}
		}

#if NET6_0
		[SupportedOSPlatform("windows")]
#endif
		private class RegistryPathBuilder : IRegistryPathBuilder
		{
			public ISettingsSourceProviderFactory Path(string companyName, string appName)
			{
				return new RegistryProviderFactory(companyName, appName);
			}
		}

		private class XmlPathBuilder : IXmlPathBuilder
		{
			public ISettingsSourceProviderFactory Path(string appPath)
			{
				return new XmlProviderFactory(appPath);
			}

			public ISettingsSourceProviderFactory LocalAppDataPath(string companyName, string appName)
			{
				var sep = System.IO.Path.DirectorySeparatorChar;

				var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{sep}{companyName}{sep}{appName}";

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				return new XmlProviderFactory($"{path}{sep}{SettingsFileName}.xml");
			}

			public ISettingsSourceProviderFactory ExecutablePath()
			{
				var sep = System.IO.Path.DirectorySeparatorChar;
				var executingPath = Assembly.GetExecutingAssembly().Location;
				var directoryPath = System.IO.Path.GetDirectoryName(executingPath);

				return new XmlProviderFactory($"{directoryPath}{sep}{SettingsFileName}.xml");
			}
		}

		#endregion
	}
}