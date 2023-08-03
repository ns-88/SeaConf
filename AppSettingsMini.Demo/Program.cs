namespace AppSettingsMini.Demo
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var service = new SettingsService(Sources.Xml.LocalAppDataPath("CompanyName", "AppName"));

			var model = service.GetModel<IProgramSettings>();

			await service.LoadAsync();

			model.StringValue = "test1 test2 test3";
			model.BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 });
			model.DoubleValue = 1.151;
			model.IntValue = 13451;
			model.LongValue = 578902319;

			await service.SaveAsync();
		}
	}
}