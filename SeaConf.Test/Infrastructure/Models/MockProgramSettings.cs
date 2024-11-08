using System.Net;

namespace SeaConf.Test.Infrastructure.Models
{
    internal class MockProgramSettings : IProgramSettings
    {
        public string? StringValue { get; set; }
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public ulong UlongValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }
        public Regime EnumValue { get; set; }
        public ReadOnlyMemory<byte> BytesValue { get; set; }
        public decimal DecimalValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public DateOnly DateOnlyValue { get; set; }
        public TimeOnly TimeOnlyValue { get; set; }
        public IPEndPoint? IpEndPointValue { get; set; }

        public IUserSettings UserSettings { get; set; } = new MockUserSettings();
    }

    internal class MockUserSettings : IUserSettings
	{
		public string? StringValue { get; set; }
		public int IntValue { get; set; }
		public long LongValue { get; set; }
		public IAddressSettings AddressSettings { get; set; } = new MockAddressSettings();
	}

    internal class MockAddressSettings : IAddressSettings
    {
	    public string? StringValue { get; set; }
	    public int IntValue { get; set; }
	    public long LongValue { get; set; }
    }
}