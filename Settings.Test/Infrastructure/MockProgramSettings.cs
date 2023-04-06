using Settings.Test.Infrastructure.Models;

namespace Settings.Test.Infrastructure
{
	internal class MockProgramSettings : IProgramSettings
	{
		public string? StringValue { get; set; }
		public int IntValue { get; set; }
		public long LongValue { get; set; }
		public double DoubleValue { get; set; }
		public ReadOnlyMemory<byte> BytesValue { get; set; }
	}
}