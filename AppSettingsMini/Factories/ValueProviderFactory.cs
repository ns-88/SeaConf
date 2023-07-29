using System;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.Interfaces.ValueProviders;
using AppSettingsMini.ValueProviders;

namespace AppSettingsMini.Factories
{
    public delegate ValueProviderBase<T> FactoryDelegate<T>(ISettingsSourceProvider provider);

	public class ValueProviderFactory<T> : IValueProviderFactory
	{
		private readonly FactoryDelegate<T> _factory;
		public Type Type => typeof(T);

		public ValueProviderFactory(FactoryDelegate<T> factory)
		{
			Guard.ThrowIfNull(factory, out _factory);
		}

		public IValueProvider Create(ISettingsSourceProvider sourceProvider)
		{
			Guard.ThrowIfNull(sourceProvider);

			var provider = _factory(sourceProvider);

			if (provider == null!)
			{
				throw new InvalidOperationException("");
			}

			return provider;
		}
	}
}