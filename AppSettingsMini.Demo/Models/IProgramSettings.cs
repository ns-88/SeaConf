using System.Net;
using AppSettingsMini.Models;

namespace AppSettingsMini.Demo.Models
{
	internal interface IProgramSettings
	{
        string? StringValue { get; set; }
        int IntValue { get; set; }
        long LongValue { get; set; }
        double DoubleValue { get; set; }
        bool BoolValue { get; set; }
        Regime EnumValue { get; set; }
        ReadOnlyMemory<byte> BytesValue { get; set; }
		decimal DecimalValue { get; set; }
		DateTime DateTimeValue { get; set; }
		DateOnly DateOnlyValue { get; set; }
		TimeOnly TimeOnlyValue { get; set; }
		IPEndPoint IpEndPointValue { get; set; }

        [Model]
		IUserSettings UserSettings { get; }

		[Model]
		IAddressSettings AddressSettings { get; }

		[Model]
		IPressureSettings PressureSettings { get; }
	}

	internal interface IUserSettings
	{
		string? StringValue { get; set; }
		int IntValue { get; set; }
		long LongValue { get; set; }
	}

	internal interface IAddressSettings
	{
		string? StringValue { get; set; }
		int IntValue { get; set; }
		long LongValue { get; set; }
	}

	internal interface IPressureSettings
	{
		string? StringValue { get; set; }
		int IntValue { get; set; }
		long LongValue { get; set; }

		[Model]
		IValveSettings ValveSettings { get; }
	}

	internal interface IValveSettings
	{
		string? StringValue { get; set; }
		int IntValue { get; set; }
		long LongValue { get; set; }
	}
}