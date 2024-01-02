using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini.Core.ValueProviders
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
				type == typeof(decimal) ||
				type == typeof(DateTime) ||
				type == typeof(DateOnly) ||
				type == typeof(TimeOnly) ||
				type == typeof(IPEndPoint) ||
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

		public bool TryCreate(Type type, [MaybeNullWhen(false)] out IValueProvider valueProvider)
		{
			Guard.ThrowIfNull(type);

			if (_providers.TryGetValue(type, out valueProvider))
			{
				return true;
			}

			if (_valueProviderFactories.TryGetValue(type, out var factory))
			{
				valueProvider = factory.Create();

				if (valueProvider == null!)
				{
					throw new InvalidOperationException(string.Format(Strings.ValueProviderNotCreated, type));
				}

				return true;
			}

			if (type == typeof(string))
			{
				valueProvider = new StringValueProvider();
			}
			else if (type == typeof(int))
			{
				valueProvider = new IntValueProvider();
			}
			else if (type == typeof(long))
			{
				valueProvider = new LongValueProvider();
			}
			else if (type == typeof(double))
			{
				valueProvider = new DoubleValueProvider();
			}
			else if (type == typeof(decimal))
			{
				valueProvider = new DecimalValueProvider();
			}
			else if (type == typeof(bool))
			{
				valueProvider = new BooleanValueProvider();
			}
			else if (type == typeof(ReadOnlyMemory<byte>))
			{
				valueProvider = new BytesValueProvider();
			}
			else if (type == typeof(DateTime))
			{
				valueProvider = new DateTimeValueProvider();
			}
			else if (type == typeof(DateOnly))
			{
				valueProvider = new DateOnlyValueProvider();
			}
			else if (type == typeof(TimeOnly))
			{
				valueProvider = new TimeOnlyValueProvider();
			}
			else if (type == typeof(IPEndPoint))
			{
				valueProvider = new IpEndPointValueProvider();
			}
			else if (type.IsEnum)
			{
				valueProvider = new EnumValueProvider();
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
		private readonly Dictionary<Type, IEqualityComparer> _comparers = new();

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