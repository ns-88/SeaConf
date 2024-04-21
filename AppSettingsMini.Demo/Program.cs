using System.Net;
using AppSettingsMini.Demo.Models;

namespace AppSettingsMini.Demo
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var service = new SettingsService(Sources.Xml.LocalAppDataPath("CompanyName", "AppName"));

			var model = service.GetModel<IProgramSettings>();

			await service.LoadAsync();

			model.StringValue = "test1 test2 test3 test4";
			model.BytesValue = new ReadOnlyMemory<byte>([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11]);
			model.DoubleValue = 1.151;
			model.IntValue = 13451;
			model.LongValue = 578902319;
			model.BoolValue = true;
			model.EnumValue = Regime.Auto;
			model.DecimalValue = 3925768910942182517;
			model.DateTimeValue = new DateTime(2024, 01, 01, 09, 10, 30);
			model.DateOnlyValue = new DateOnly(2024, 01, 01);
			model.TimeOnlyValue = new TimeOnly(09, 10, 30);
			model.IpEndPointValue = new IPEndPoint(IPAddress.Parse("192.168.0.1"), 9001);

			model.UserSettings.IntValue = 237;
			model.UserSettings.StringValue = "UserStringValue";
			model.UserSettings.LongValue = 578902319;

			model.AddressSettings.IntValue = 238;
			model.AddressSettings.StringValue = "AddressStringValue";
			model.AddressSettings.LongValue = 578902319;

			model.PressureSettings.IntValue = 239;
			model.PressureSettings.StringValue = "PressureStringValue";
			model.PressureSettings.LongValue = 578902319;

			model.PressureSettings.ValveSettings.IntValue = 240;
			model.PressureSettings.ValveSettings.StringValue = "ValveStringValue";
			model.PressureSettings.ValveSettings.LongValue = 578902319;

			service.StreetSettings.IntValue = 241;
			service.StreetSettings.StringValue = "StreetStringValue";
			service.StreetSettings.LongValue = 578902319;

			service.StreetSettings.IntValue = 242;
			service.StreetSettings.StringValue = "StreetStringValue";
			service.StreetSettings.LongValue = 578902319;

			service.StreetSettings.HomeSettings.IntValue = 243;
			service.StreetSettings.HomeSettings.StringValue = "HomeStringValue";
			service.StreetSettings.HomeSettings.LongValue = 578902319;

			await service.SaveAsync();
		}
	}
}