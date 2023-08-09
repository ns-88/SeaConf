using System;
using System.Collections;
using AppSettingsMini.Interfaces.ValueProviders;

namespace AppSettingsMini.Interfaces.Factories
{
	public interface IValueProviderFactory
    {
        Type Type { get; }
        IEqualityComparer Comparer { get; }
        IValueProvider Create(ISettingsSourceProvider sourceProvider);
    }
}