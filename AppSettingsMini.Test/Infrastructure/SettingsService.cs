using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.Test.Infrastructure.Models;

namespace AppSettingsMini.Test.Infrastructure
{
	internal class SettingsService : SettingsServiceBase
	{
		public IProgramSettings ProgramSettings { get; }

		public SettingsService(ISourceFactory<IStorageModel> sourceFactory)
			: base(sourceFactory)
		{
			Register(x => x.RegisterModel<IProgramSettings, ProgramSettings>());
			ProgramSettings = GetModel<IProgramSettings>();
		}
	}
}