using System.Net;
using AppSettingsMini.Models;

namespace AppSettingsMini.Demo.Models
{
	internal class ProgramSettings : ModelBase, IProgramSettings
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

		public double DoubleValue
		{
			get => GetValue<double>();
			set => SetValue(value);
		}

		public bool BoolValue
		{
			get => GetValue<bool>();
			set => SetValue(value);
		}

		public Regime EnumValue
		{
			get => GetValue<Regime>();
			set => SetValue(value);
		}

		public ReadOnlyMemory<byte> BytesValue
		{
			get => GetValue<ReadOnlyMemory<byte>>();
			set => SetValue(value);
		}

		public decimal DecimalValue
		{
			get => GetValue<decimal>();
			set => SetValue(value);
		}

		public DateTime DateTimeValue
		{
			get => GetValue<DateTime>();
			set => SetValue(value);
		}

		public DateOnly DateOnlyValue
		{
			get => GetValue<DateOnly>();
			set => SetValue(value);
		}

		public TimeOnly TimeOnlyValue
		{
			get => GetValue<TimeOnly>();
			set => SetValue(value);
		}

		public IPEndPoint IpEndPointValue
		{
			get => GetValue<IPEndPoint>();
			set => SetValue(value);
		}

		public IUserSettings UserSettings { get; } = new UserSettings();

		public IAddressSettings AddressSettings { get; } = new AddressSettings();

		public IPressureSettings PressureSettings { get; } = new PressureSettings();
	}

	internal class UserSettings : ModelBase, IUserSettings
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

	internal class AddressSettings : ModelBase, IAddressSettings
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

	internal class PressureSettings : ModelBase, IPressureSettings
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

		public IValveSettings ValveSettings { get; } = new ValveSettings();
	}

	internal class ValveSettings : ModelBase, IValveSettings
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