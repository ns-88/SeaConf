using SeaConf.Models;

namespace SeaConf.Demo.Models
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