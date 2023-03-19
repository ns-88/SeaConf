using Settings.RegistrySource;

namespace Settings.Demo
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var provider = new RegistrySettingsSourcesProvider("OrgName\\AppName");
			var service = new SettingsService(provider)
			{
				//ProgramSettings =
				//{
				//	BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
				//	IntValue = 25,
				//	LongValue = 701089,
				//	StringValue = "Test test test"
				//}
			};

			await service.LoadAsync();

			var value = service.ProgramSettings.LongValue;
		}
	}
}