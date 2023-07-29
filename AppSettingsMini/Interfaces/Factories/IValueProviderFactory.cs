using System;
using AppSettingsMini.Interfaces.ValueProviders;

namespace AppSettingsMini.Interfaces.Factories
{
    public interface IValueProviderFactory
    {
        Type Type { get; }
        IValueProvider Create(ISettingsSourceProvider sourceProvider);
    }
}