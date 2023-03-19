namespace Settings.Demo
{
	internal interface IProgramSettings
	{
		string? StringValue { get; set; }
		int IntValue { get; set; }
		long LongValue { get; set; }
		double DoubleValue { get; }
		ReadOnlyMemory<byte> BytesValue { get; set; }
	}
}