using System;
using System.Collections;
using System.Collections.Generic;
using AppSettingsMini.Core.ValueProviders;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini.Core.Factories
{
	public class ValueProviderFactory<T> : IValueProviderFactory
	{
		private readonly Func<ValueProviderBase<T>> _factory;
		public Type Type => typeof(T);
		public IEqualityComparer Comparer { get; }

		public ValueProviderFactory(Func<ValueProviderBase<T>> factory, IEqualityComparer? comparer = null)
		{
			_factory = Guard.ThrowIfNull(factory);
			Comparer = comparer ?? EqualityComparer<T>.Default;
		}

		public IValueProvider Create()
		{
			var provider = _factory();

			if (provider == null!)
			{
				throw new InvalidOperationException(string.Format(Strings.ValueProviderNotCreated, Type));
			}

			return provider;
		}
	}
}