using System.Threading.Tasks;
using System;

namespace AppSettingsMini.Interfaces.Core
{
    public interface IWriter : IAsyncDisposable
    {
        ValueTask WriteStringAsync(string value, string propertyName);
        ValueTask WriteIntAsync(int value, string propertyName);
        ValueTask WriteLongAsync(long value, string propertyName);
        ValueTask WriteDoubleAsync(double value, string propertyName);
        ValueTask WriteDecimalAsync(decimal value, string propertyName);
        ValueTask WriteBooleanAsync(bool value, string propertyName);
        ValueTask WriteBytesAsync(ReadOnlyMemory<byte> value, string propertyName);
    }
}