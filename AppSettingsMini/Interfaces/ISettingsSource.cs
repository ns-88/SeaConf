using System;
using System.Threading.Tasks;

namespace AppSettingsMini.Interfaces
{
    public interface ISettingsSource
    {
        ValueTask<bool> CollectionExistsAsync(string collectionName);
        ValueTask<bool> PropertyExistsAsync(string collectionName, string propertyName);
    }

    public interface IReadableSettingsSource : ISettingsSource
    {
        ValueTask<string> GetStringValueAsync(string collectionName, string propertyName);
        ValueTask<int> GetIntValueAsync(string collectionName, string propertyName);
        ValueTask<long> GetLongValueAsync(string collectionName, string propertyName);
        ValueTask<double> GetDoubleValueAsync(string collectionName, string propertyName);
        ValueTask<ReadOnlyMemory<byte>> GetBytesValueAsync(string collectionName, string propertyName);
	}

    public interface IWriteableSettingsSource : ISettingsSource
    {
        ValueTask SetStringValueAsync(string value, string collectionName, string propertyName);
        ValueTask SetIntValueAsync(int value, string collectionName, string propertyName);
        ValueTask SetLongValueAsync(long value, string collectionName, string propertyName);
		ValueTask SetDoubleValueAsync(double value, string collectionName, string propertyName);
        ValueTask SetBytesValueAsync(ReadOnlyMemory<byte> value, string collectionName, string propertyName);
		ValueTask DeletePropertyAsync(string collectionName, string propertyName);
    }
}