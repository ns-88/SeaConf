using Settings.RegistrySource;

namespace Settings.Demo
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var provider = new RegistrySettingsSourcesProvider("OrgName\\AppName");
			var service = new SettingsService(provider);

			await service.LoadAsync();
		}
	}
}