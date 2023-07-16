namespace AppSettingsMini.Demo
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var service = new SettingsService(Sources.FromXml("D:\\Settings.xml"));
			
			var model = service.GetModel<IProgramSettings>();

			await service.LoadAsync();

			model.StringValue = "test1 test2 test3";
			model.BytesValue = new ReadOnlyMemory<byte>(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			model.DoubleValue = 1.15;
			model.IntValue = 1345;
			model.LongValue = 57890231;

			await service.SaveAsync();
		}
	}
}