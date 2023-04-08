using AppSettingsMini.Interfaces;
using Moq;

namespace AppSettingsMini.Test.Infrastructure
{
	internal class MockSettingsSourceProvider : ISettingsSourceProvider
    {
	    public IReadableSettingsSource ReadableSettingsStore { get; }
	    public IWriteableSettingsSource WriteableSettingsStore { get; }

	    private MockSettingsSourceProvider(IReadableSettingsSource readableSettingsStore, IWriteableSettingsSource writeableSettingsStore)
	    {
		    ReadableSettingsStore = readableSettingsStore;
		    WriteableSettingsStore = writeableSettingsStore;
	    }

	    public static MockSettingsSourceProvider CreateForReadableSource(out Mock<IReadableSettingsSource> mock)
	    {
		    var readableSourceMock = new Mock<IReadableSettingsSource>();
		    var writeableSourceMock = new Mock<IWriteableSettingsSource>();

		    mock = readableSourceMock;

		    return new MockSettingsSourceProvider(readableSourceMock.Object, writeableSourceMock.Object);
	    }

	    public static MockSettingsSourceProvider CreateForWriteableSource(out Mock<IWriteableSettingsSource> mock)
	    {
		    var readableSourceMock = new Mock<IReadableSettingsSource>();
		    var writeableSourceMock = new Mock<IWriteableSettingsSource>();

		    mock = writeableSourceMock;

		    return new MockSettingsSourceProvider(readableSourceMock.Object, writeableSourceMock.Object);
	    }
	}
}