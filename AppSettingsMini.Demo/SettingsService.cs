using AppSettingsMini.Demo.Models;
using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini.Demo
{
	internal class SettingsService : SettingsServiceBase
	{
		public IProgramSettings ProgramSettings { get; }

		public SettingsService(ISettingsSourceProviderFactory sourceProviderFactory)
			: base(sourceProviderFactory)
		{
			ProgramSettings = RegisterModel<IProgramSettings, ProgramSettings>();
		}
	}
}