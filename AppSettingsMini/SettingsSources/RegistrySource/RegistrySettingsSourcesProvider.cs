using System.Runtime.Versioning;

namespace AppSettingsMini.SettingsSources.RegistrySource
{
#if NET6_0
	[SupportedOSPlatform("windows")]
#endif
	internal class RegistrySettingsSourcesProvider : SettingsSourceProviderBase
	{
		public RegistrySettingsSourcesProvider(string companyName, string appName, string rootKeyName)
			: base(new RegistrySettingsSource(companyName, appName, rootKeyName))
		{
		}
	}
}