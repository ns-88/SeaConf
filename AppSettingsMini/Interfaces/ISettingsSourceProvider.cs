using System;
using System.Threading.Tasks;

namespace AppSettingsMini.Interfaces
{
	public interface ISettingsSourceProvider : IDisposable, IAsyncDisposable
    {
        IReadableSettingsSource ReadableSettingsSource { get; }
        IWriteableSettingsSource WriteableSettingsSource { get; }

        ValueTask LoadAsync();
        ValueTask SaveAsync();
    }
}