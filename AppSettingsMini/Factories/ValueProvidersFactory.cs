using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.Interfaces.ValueProviders;
using AppSettingsMini.ValueProviders;

namespace AppSettingsMini.Factories
{
	using ValueProviderFactories = IReadOnlyDictionary<Type, IValueProviderFactory>;

	internal readonly struct ValueProvidersFactory : IDisposable
	{
		private readonly Dictionary<Type, IValueProvider> _providers;
		private readonly ValueProviderFactories _valueProviderFactories;

		public ValueProvidersFactory(ValueProviderFactories valueProviderFactories)
		{
			_valueProviderFactories = valueProviderFactories;
			_providers = new Dictionary<Type, IValueProvider>();
		}

		public bool TryCreate(Type type, ISettingsSourceProvider sourceProvider, [MaybeNullWhen(false)] out IValueProvider valueProvider)
		{
			Guard.ThrowIfNull(type);
			Guard.ThrowIfNull(sourceProvider);

			if (_providers.TryGetValue(type, out valueProvider))
			{
				return true;
			}

			if (_valueProviderFactories.TryGetValue(type, out var factory))
			{
				valueProvider = factory.Create(sourceProvider);

				if (valueProvider == null!)
				{
					throw new InvalidOperationException("");
				}

				return true;
			}

			if (type == typeof(string))
			{
				valueProvider = new StringValueProvider(sourceProvider);
			}
			else if (type == typeof(int))
			{
				valueProvider = new IntValueProvider(sourceProvider);
			}
			else if (type == typeof(long))
			{
				valueProvider = new LongValueProvider(sourceProvider);
			}
			else if (type == typeof(double))
			{
				valueProvider = new DoubleValueProvider(sourceProvider);
			}
			else if (type == typeof(ReadOnlyMemory<byte>))
			{
				valueProvider = new BytesValueProvider(sourceProvider);
			}

			if (valueProvider == null)
			{
				return false;
			}

			_providers.Add(type, valueProvider);

			return true;
		}

		public void Dispose()
		{
			_providers.Clear();
		}
	}
}