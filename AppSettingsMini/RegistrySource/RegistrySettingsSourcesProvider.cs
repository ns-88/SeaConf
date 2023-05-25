using System.Runtime.Versioning;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini.RegistrySource
{
#if NET6_0
	[SupportedOSPlatform("windows")]
#endif
	public class RegistrySettingsSourcesProvider : ISettingsSourceProvider
	{
		public IReadableSettingsSource ReadableSettingsStore { get; }
		public IWriteableSettingsSource WriteableSettingsStore { get; }

		public RegistrySettingsSourcesProvider(string appPath)
		{
			var settingsStore = new RegistrySettingsSource(appPath);

			ReadableSettingsStore = settingsStore;
			WriteableSettingsStore = settingsStore;
		}
	}
}