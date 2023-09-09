using AppSettingsMini.Test.Infrastructure.Models;

namespace AppSettingsMini.Test.Infrastructure
{
	internal class MockProgramSettings : IProgramSettings
	{
		public string? StringValue { get; set; }
		public int IntValue { get; set; }
		public long LongValue { get; set; }
		public double DoubleValue { get; set; }
		public bool BoolValue { get; set; }
		public Regime EnumValue { get; set; }
		public ReadOnlyMemory<byte> BytesValue { get; set; }
	}
}