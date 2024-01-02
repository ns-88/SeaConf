using System;
using System.Threading.Tasks;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Core;

namespace AppSettingsMini.Core.ValueProviders
{
    public abstract class ValueProviderBase<T> : IValueProvider
    {
        public Type Type => typeof(T);

        public abstract ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo);

        public abstract ValueTask SetAsync(IWriter writer, IPropertyData propertyData);
    }
}