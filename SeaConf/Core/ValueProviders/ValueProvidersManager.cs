using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using SeaConf.Infrastructure;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Factories;

namespace SeaConf.Core.ValueProviders
{
    internal class ValueProvidersManager
    {
        private readonly IReadOnlyDictionary<Type, IValueProviderFactory> _valueProviderFactories;
        private readonly Dictionary<Type, IEqualityComparer> _comparers;

        public ValueProvidersManager(IReadOnlyDictionary<Type, IValueProviderFactory> valueProviderFactories)
        {
            _valueProviderFactories = valueProviderFactories;
            _comparers = new Dictionary<Type, IEqualityComparer>();
        }

        public void ThrowIfNotSupportedType(Type type)
        {
            var isSupportedType = type == typeof(string) || 
                                  type == typeof(int) ||
                                  type == typeof(long) ||
                                  type == typeof(ulong) ||
                                  type == typeof(double) ||
                                  type == typeof(bool) ||
                                  type == typeof(decimal) ||
                                  type == typeof(DateTime) ||
                                  type == typeof(DateOnly) ||
                                  type == typeof(TimeOnly) ||
                                  type == typeof(IPEndPoint) ||
                                  type.IsEnum ||
                                  type.IsReadOnlyByteMemory() ||
                                  _valueProviderFactories.ContainsKey(type);

            if (!isSupportedType)
            {
                throw new InvalidOperationException(string.Format(Strings.TypeNotSupported, type));
            }
        }

        public ValueProvidersFactory CreateFactory()
        {
            return new ValueProvidersFactory(_valueProviderFactories);
        }

        public IEqualityComparer<T> GetComparer<T>()
        {
            var type = typeof(T);

            if (_comparers.TryGetValue(type, out var comparer))
            {
                return (IEqualityComparer<T>)comparer;
            }

            if (_valueProviderFactories.TryGetValue(type, out var factory))
            {
                comparer = factory.Comparer;
            }
            else
            {
                comparer = type.IsReadOnlyByteMemory()
                    ? BytesValueProvider.GetComparer()
                    : ValueProviderBase<T>.GetComparer();
            }

            _comparers.Add(type, comparer);

            return (IEqualityComparer<T>)comparer;
        }
    }

    internal readonly struct ValueProvidersFactory : IDisposable
    {
        private readonly Dictionary<Type, IValueProvider> _providers;
        private readonly IReadOnlyDictionary<Type, IValueProviderFactory> _valueProviderFactories;

        public ValueProvidersFactory(IReadOnlyDictionary<Type, IValueProviderFactory> valueProviderFactories)
        {
            _valueProviderFactories = valueProviderFactories;
            _providers = new Dictionary<Type, IValueProvider>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreate(Type type, [MaybeNullWhen(false)] out IValueProvider valueProvider)
        {
            Guard.ThrowIfNull(type);
            
            if (_providers.TryGetValue(ValueProviderBase.GetRealType(type), out valueProvider))
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
            }
            else
            {
                valueProvider = ValueProviderBase.Create(ref type);
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