using AppSettingsMini.Interfaces;
using AppSettingsMini.Test.Infrastructure.Models;

namespace AppSettingsMini.Test.Infrastructure
{
	internal class SettingsService : SettingsServiceBase
	{
		public IProgramSettings ProgramSettings { get; }

		public SettingsService(ISettingsSourceProvider sourceProvider, string? collectionName = null)
			: base(sourceProvider, collectionName)
		{
			ProgramSettings = RegisterModel<IProgramSettings, ProgramSettings>("Настройки приложения", false);
		}
	}
}