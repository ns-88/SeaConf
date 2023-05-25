using AppSettingsMini.Interfaces;

namespace AppSettingsMini.Demo
{
	internal class SettingsService : SettingsServiceBase
	{
		public IProgramSettings ProgramSettings { get; }

		public SettingsService(ISettingsSourceProvider settingsProvider)
			: base(settingsProvider)
		{
			ProgramSettings = RegisterModel<IProgramSettings, ProgramSettings>("Настройки приложения", false);
		}
	}
}