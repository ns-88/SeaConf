using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

		public ValueProvidersFactory CreateFactory()
		{
			return new ValueProvidersFactory(_valueProviderFactories);
		}

		public void AddFactory(IValueProviderFactory factory)
		{
			Guard.ThrowIfNull(factory);
			Guard.ThrowIfNull(factory.Type);

			if (ValueProviderBase.IsSupportedType(factory.Type) || _valueProviderFactories.ContainsKey(factory.Type))
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

			if (_providers.TryGetValue(ValueProviderBase.GetKeyType(type), out valueProvider))
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

			valueProvider = ValueProviderBase.Create(type, out var keyType);

			if (valueProvider == null)
			{
				return false;
			}

			_providers.Add(keyType, valueProvider);

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
}