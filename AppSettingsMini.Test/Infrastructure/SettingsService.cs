using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.Test.Infrastructure.Models;

namespace AppSettingsMini.Test.Infrastructure
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