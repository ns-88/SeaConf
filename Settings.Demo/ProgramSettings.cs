namespace Settings.Demo
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

		public double DoubleValue => GetValue<double>();

		public ReadOnlyMemory<byte> BytesValue
		{
			get => GetValue<ReadOnlyMemory<byte>>();
			set => SetValue(value);
		}
	}
}