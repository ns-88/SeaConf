using AppSettingsMini.Models;

namespace AppSettingsMini.Demo.Models
{
	internal interface IStreetSettings
	{
		string? StringValue { get; set; }
		int IntValue { get; set; }
		long LongValue { get; set; }

		[Model]
		IHomeSettings HomeSettings { get; }
	}

	internal interface IHomeSettings
	{
		string? StringValue { get; set; }
		int IntValue { get; set; }
		long LongValue { get; set; }
	}
}