namespace Settings.Interfaces
{
    public interface ISettingsSourceProvider
    {
        IReadableSettingsSource ReadableSettingsStore { get; }
        IWriteableSettingsSource WriteableSettingsStore { get; }
    }
}