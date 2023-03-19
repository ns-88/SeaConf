using Settings.Interfaces;

namespace Settings.Demo
{
    internal class SettingsService : SettingsServiceBase
	{
		public IProgramSettings ProgramSettings { get; }

		public SettingsService(ISettingsSourceProvider settingsProvider)
			: base(settingsProvider)
		{
			ProgramSettings = RegisterModel<ProgramSettings>("Настройки приложения", false);
		}
	}
}