using System;
using System.Threading.Tasks;
using AppSettingsMini.Interfaces.Core;

namespace AppSettingsMini.Interfaces
{
    public interface IValueProvider
    {
        Type Type { get; }
        ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo);
        ValueTask SetAsync(IWriter writer, IPropertyData propertyData);
    }
}