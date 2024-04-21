using AppSettingsMini.Models;

namespace AppSettingsMini.Demo.Models
{
	internal class StreetSettings : ModelBase, IStreetSettings
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

	internal class HomeSettings : ModelBase, IHomeSettings
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