using System;
using System.Threading.Tasks;

namespace AppSettingsMini.Interfaces.ValueProviders
{
	public interface IValueProvider
    {
        Type Type { get; }
        ValueTask<ISettingsPropertyData> GetAsync(string collectionName, ISettingsPropertyInfo propertyInfo);
        ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData);
    }
}