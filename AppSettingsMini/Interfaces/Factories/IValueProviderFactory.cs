using System;
using System.Collections;

namespace AppSettingsMini.Interfaces.Factories
{
    public interface IValueProviderFactory
    {
        Type Type { get; }
        IEqualityComparer Comparer { get; }
        IValueProvider Create();
    }
}