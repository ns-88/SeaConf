namespace AppSettingsMini.Demo
{
	internal class ProgramSettings : SettingsModelBase, IProgramSettings
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
            get=> GetValue<double>();
            set => SetValue(value);
        }

        public ReadOnlyMemory<byte> BytesValue
        {
            get => GetValue<ReadOnlyMemory<byte>>();
            set => SetValue(value);
        }
    }
}