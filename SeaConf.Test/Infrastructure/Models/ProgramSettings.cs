using System.Net;
using SeaConf.Models;

namespace SeaConf.Test.Infrastructure.Models
{
	internal class ProgramSettings : PropertiesModel, IProgramSettings
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

        public ulong UlongValue
        {
            get => GetValue<ulong>();
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

		public IPEndPoint? IpEndPointValue
		{
			get => GetValue<IPEndPoint>();
			set => SetValue(value);
		}

		public IUserSettings UserSettings { get; } = new UserSettings();
	}

	internal class UserSettings : PropertiesModel, IUserSettings
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

		public IAddressSettings AddressSettings { get; } = new AddressSettings();
	}

	internal class AddressSettings : PropertiesModel, IAddressSettings
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