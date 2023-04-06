using Settings.Interfaces;
using Settings.Test.Infrastructure.Models;

namespace Settings.Test.Infrastructure
{
    internal class SettingsService : SettingsServiceBase
	{
		public IProgramSettings ProgramSettings { get; }

		public SettingsService(ISettingsSourceProvider sourceProvider, string? collectionName = null)
			: base(sourceProvider, collectionName)
		{
			ProgramSettings = RegisterModel<ProgramSettings>("Настройки приложения", false);
		}
	}
}