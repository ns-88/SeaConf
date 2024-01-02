using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini
{
	public static class Sources
	{
		[SupportedOSPlatform("windows")]
		public static IRegistryPathBuilder Registry { get; } = new RegistryPathBuilder();

		public static IXmlPathBuilder Xml { get; } = new XmlPathBuilder();
	}

	[SupportedOSPlatform("windows")]
	file class RegistryPathBuilder : IRegistryPathBuilder
	{
		public ISourceFactory<IStorageModel> Path(string companyName, string appName)
		{
			return new RegistrySourceFactory(companyName, appName);
		}
	}

	file class XmlPathBuilder : IXmlPathBuilder
	{
		public ISourceFactory<IStorageModel> Path(string appPath)
		{
			return new XmlSourceFactory(appPath);
		}

		public ISourceFactory<IStorageModel> LocalAppDataPath(string companyName, string appName)
		{
			var sep = System.IO.Path.DirectorySeparatorChar;

			var path = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{sep}{companyName}{sep}{appName}";

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return new XmlSourceFactory($"{path}{sep}{Strings.SettingsFileName}.xml");
		}

		public ISourceFactory<IStorageModel> ExecutablePath()
		{
			var sep = System.IO.Path.DirectorySeparatorChar;
			var executingPath = Assembly.GetExecutingAssembly().Location;
			var directoryPath = System.IO.Path.GetDirectoryName(executingPath);

			return new XmlSourceFactory($"{directoryPath}{sep}{Strings.SettingsFileName}.xml");
		}
	}
}