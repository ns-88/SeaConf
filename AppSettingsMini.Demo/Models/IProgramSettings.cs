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
    }
}