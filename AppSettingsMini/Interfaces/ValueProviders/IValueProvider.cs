using System;
using System.Threading.Tasks;

namespace AppSettingsMini.Interfaces.ValueProviders
{
	public interface IValueProvider
    {
        Type Type { get; }

	    ValueTask<IPropertyData> GetAsync(string collectionName, string propertyName);
        ValueTask SetAsync(string collectionName, IPropertyData propertyData);
    }
}