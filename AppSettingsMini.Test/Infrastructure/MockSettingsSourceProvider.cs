using AppSettingsMini.Interfaces;

namespace AppSettingsMini.Test.Infrastructure
{
	internal class MockSettingsSourceProvider : ISettingsSourceProvider
	{
		public IReadableSettingsSource ReadableSettingsSource { get; }
		public IWriteableSettingsSource WriteableSettingsSource { get; }

		public MockSettingsSourceProvider(IReadableSettingsSource readableSettingsStore, IWriteableSettingsSource writeableSettingsStore)
		{
			ReadableSettingsSource = readableSettingsStore;
			WriteableSettingsSource = writeableSettingsStore;
		}

		public ValueTask LoadAsync()
		{
			return new ValueTask();
		}

		public ValueTask SaveAsync()
		{
			return new ValueTask();
		}

		public void Dispose()
		{
		}

		public ValueTask DisposeAsync()
		{
			return new ValueTask();
		}
	}
}