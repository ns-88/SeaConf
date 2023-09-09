using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;
using AppSettingsMini.Interfaces.ValueProviders;
using AppSettingsMini.ValueProviders;

namespace AppSettingsMini
{
	using ValueProviderFactories = IReadOnlyDictionary<Type, IValueProviderFactory>;

	internal readonly struct ValueProvidersManager
	{
		private readonly Dictionary<Type, IValueProviderFactory> _valueProviderFactories;
		public readonly ComparersManager ComparersManager;

		public ValueProvidersManager()
		{
			_valueProviderFactories = new Dictionary<Type, IValueProviderFactory>();
			ComparersManager = new ComparersManager();
		}

		private bool Contains(Type type)
		{
			if (type == typeof(string) ||
				type == typeof(int) ||
				type == typeof(long) ||
				type == typeof(double) ||
				type == typeof(bool) ||
				type.IsEnum ||
				type.IsReadOnlyByteMemory())
			{
				return true;
			}

			return _valueProviderFactories.ContainsKey(type);
		}

		public ValueProvidersFactory CreateFactory()
		{
			return new ValueProvidersFactory(_valueProviderFactories);
		}

		public void AddFactory(IValueProviderFactory factory)
		{
			Guard.ThrowIfNull(factory);
			Guard.ThrowIfNull(factory.Type);

			if (Contains(factory.Type))
			{
				throw new InvalidOperationException(string.Format(Strings.ValueProviderAlreadyRegistered, factory.Type.Name));
			}

			_valueProviderFactories.Add(factory.Type, factory);

			ComparersManager.Add(factory.Type, factory.Comparer);
		}
	}

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
					throw new InvalidOperationException(string.Format(Strings.ValueProviderNotCreated, type));
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
			else if (type == typeof(bool))
			{
				valueProvider = new BooleanValueProvider(sourceProvider);
			}
			else if (type == typeof(ReadOnlyMemory<byte>))
			{
				valueProvider = new BytesValueProvider(sourceProvider);
			}
			else if (type.IsEnum)
			{
				valueProvider = new EnumValueProvider(sourceProvider);
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

	internal class ComparersManager
	{
		private readonly Dictionary<Type, IEqualityComparer> _comparers;

		public ComparersManager()
		{
			_comparers = new Dictionary<Type, IEqualityComparer>();
		}

		public void Add(Type type, IEqualityComparer comparer)
		{
			_comparers.Add(type, comparer);
		}

		public IEqualityComparer<T> Get<T>()
		{
			var type = typeof(T);

			if (_comparers.TryGetValue(type, out var comparer))
			{
				return (IEqualityComparer<T>)comparer;
			}

			if (type.IsReadOnlyByteMemory())
			{
				comparer = new ReadOnlyMemoryByteComparer();
			}
			else
			{
				comparer = EqualityComparer<T>.Default;
			}

			return (IEqualityComparer<T>)comparer;
		}
	}

	file static class TypeExtension
	{
		public static bool IsReadOnlyByteMemory(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReadOnlyMemory<byte>);
		}
	}
}