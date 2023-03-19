using Settings.Interfaces;

namespace Settings.RegistrySource
{
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