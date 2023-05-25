using AppSettingsMini.RegistrySource;

namespace AppSettingsMini.Demo
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var provider = new RegistrySettingsSourcesProvider("OrgName\\AppName");
			var service = new SettingsService(provider);

			var model = service.GetModel<IProgramSettings>();

			await service.LoadAsync();
		}
	}
}