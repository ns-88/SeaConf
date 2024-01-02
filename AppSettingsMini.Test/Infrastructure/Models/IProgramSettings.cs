using System.Net;
using AppSettingsMini.Models;

namespace AppSettingsMini.Test.Infrastructure.Models
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
        IPEndPoint? IpEndPointValue { get; set; }

        [Model]
        IUserSettings UserSettings { get; }
    }

    internal interface IUserSettings
    {
	    string? StringValue { get; set; }
	    int IntValue { get; set; }
	    long LongValue { get; set; }

        [Model]
        IAddressSettings AddressSettings { get; }
    }

    internal interface IAddressSettings
    {
	    string? StringValue { get; set; }
	    int IntValue { get; set; }
	    long LongValue { get; set; }
    }
}