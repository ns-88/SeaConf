namespace AppSettingsMini.Test.Infrastructure.Models
{
    internal interface IProgramSettings
    {
        string? StringValue { get; set; }
        int IntValue { get; set; }
        long LongValue { get; set; }
        double DoubleValue { get; set; }
        ReadOnlyMemory<byte> BytesValue { get; set; }
    }
}