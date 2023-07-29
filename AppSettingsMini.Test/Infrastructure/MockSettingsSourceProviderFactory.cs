using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;
using Moq;

namespace AppSettingsMini.Test.Infrastructure
{
	internal class MockSettingsSourceProviderFactory : ISettingsSourceProviderFactory
	{
		private readonly ISettingsSourceProvider _provider;

		private MockSettingsSourceProviderFactory(ISettingsSourceProvider provider)
		{
			_provider = provider;
		}

		public ISettingsSourceProvider Create()
		{
			return _provider;
		}

		public static MockSettingsSourceProviderFactory CreateForReadableSource(out Mock<IReadableSettingsSource> mock)
		{
			var readableSourceMock = new Mock<IReadableSettingsSource>();
			var writeableSourceMock = new Mock<IWriteableSettingsSource>();

			mock = readableSourceMock;

			var provider = new MockSettingsSourceProvider(readableSourceMock.Object, writeableSourceMock.Object);

			return new MockSettingsSourceProviderFactory(provider);
		}

		public static MockSettingsSourceProviderFactory CreateForWriteableSource(out Mock<IWriteableSettingsSource> mock)
		{
			var readableSourceMock = new Mock<IReadableSettingsSource>();
			var writeableSourceMock = new Mock<IWriteableSettingsSource>();

			mock = writeableSourceMock;

			var provider = new MockSettingsSourceProvider(readableSourceMock.Object, writeableSourceMock.Object);

			return new MockSettingsSourceProviderFactory(provider);
		}
	}
}