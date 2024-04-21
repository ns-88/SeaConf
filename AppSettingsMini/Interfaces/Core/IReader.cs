using System.Threading.Tasks;
using System;

namespace AppSettingsMini.Interfaces.Core
{
    public interface IReader : IAsyncDisposable
    {
        ValueTask<string> ReadStringAsync(string propertyName);
        ValueTask<int> ReadIntAsync(string propertyName);
        ValueTask<long> ReadLongAsync(string propertyName);
        ValueTask<double> ReadDoubleAsync(string propertyName);
        ValueTask<decimal> ReadDecimalAsync(string propertyName);
		ValueTask<bool> ReadBooleanAsync(string propertyName);
        ValueTask<ReadOnlyMemory<byte>> ReadBytesAsync(string propertyName);
        ValueTask<bool> PropertyExistsAsync(string propertyName);
    }
}