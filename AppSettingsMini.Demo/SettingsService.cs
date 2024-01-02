using AppSettingsMini.Demo.Models;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini.Demo
{
	internal class SettingsService : SettingsServiceBase
	{
		public IProgramSettings ProgramSettings { get; }
		public IStreetSettings StreetSettings { get; }

		public SettingsService(ISourceFactory<IStorageModel> sourceFactory)
			: base(sourceFactory)
		{
			Register(x =>
			{
				x.RegisterModel<IProgramSettings, ProgramSettings>();
				x.RegisterModel<IStreetSettings, StreetSettings>();
			});

			ProgramSettings = GetModel<IProgramSettings>();
			StreetSettings = GetModel<IStreetSettings>();
		}
	}
}