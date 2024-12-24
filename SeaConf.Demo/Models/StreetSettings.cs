using SeaConf.Models;

namespace SeaConf.Demo.Models
{
	internal class StreetSettings : PropertiesModel, IStreetSettings
	{
		public string? StringValue
		{
			get => GetValue<string>();
			set => SetValue(value);
		}

		public int IntValue
		{
			get => GetValue<int>();
			set => SetValue(value);
		}

		public long LongValue
		{
			get => GetValue<long>();
			set => SetValue(value);
		}

		public IHomeSettings HomeSettings { get; } = new HomeSettings();
	}

	internal class HomeSettings : PropertiesModel, IHomeSettings
	{
		public string? StringValue
		{
			get => GetValue<string>();
			set => SetValue(value);
		}

		public int IntValue
		{
			get => GetValue<int>();
			set => SetValue(value);
		}

		public long LongValue
		{
			get => GetValue<long>();
			set => SetValue(value);
		}
	}
}