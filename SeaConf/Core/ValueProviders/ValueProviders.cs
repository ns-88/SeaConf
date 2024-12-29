using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using SeaConf.Core.Sources;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;

namespace SeaConf.Core.ValueProviders
{
    /// <summary>
    /// Provider for setting and getting data of a specific type.
    /// </summary>
    public abstract class ValueProviderBase : IValueProvider
	{
        /// <summary>
        /// Supported data type.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public abstract ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo);

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
        public abstract ValueTask SetAsync(IWriter writer, IPropertyData propertyData);

		internal static Type GetRealType(Type type)
		{
			var realType = type;

			if (type.IsEnum)
			{
				realType = typeof(Enum);
			}

			return realType;
		}

		internal static IValueProvider? Create(ref Type type)
		{
			IValueProvider? valueProvider = null;

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
            else if (type == typeof(ulong))
            {
                valueProvider = new UlongValueProvider();
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
				type = typeof(Enum);
			}

			return valueProvider;
		}
	}

    /// <summary>
    /// Provider for setting and getting data of a specific type.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public abstract class ValueProviderBase<T> : ValueProviderBase
	{
        /// <summary>
        /// Supported data type.
        /// </summary>
        public override Type Type { get; } = typeof(T);

        /// <summary>
        /// Getting a comparer for a supported type.
        /// </summary>
        /// <returns>Comparer.</returns>
        public static IEqualityComparer GetComparer()
        {
            return EqualityComparer<T>.Default;
        }
    }

    /// <summary>
    /// Provider for setting and getting data of a <see cref="string"/> type.
    /// </summary>
    internal class StringValueProvider : ValueProviderBase<string>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = string.Empty;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
			{
				var rawValue = await reader.ReadStringAsync(propertyInfo, value).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    value = rawValue;
                }
			}

            return PropertyData<string>.Create(value, propertyInfo.Name);
        }

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteStringAsync(propertyData.ToTyped<string>().Get(), propertyData);
		}
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="int"/> type.
    /// </summary>
    internal class IntValueProvider : ValueProviderBase<int>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = int.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
			{
				value = await reader.ReadIntAsync(propertyInfo, value).ConfigureAwait(false);
			}

            return PropertyData<int>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteIntAsync(propertyData.ToTyped<int>().Get(), propertyData);
        }
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="ulong"/> type.
    /// </summary>
    internal class UlongValueProvider : ValueProviderBase<ulong>
    {
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
        {
            var value = ulong.MinValue;

            if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
            {
                value = await reader.ReadUlongAsync(propertyInfo, value).ConfigureAwait(false);
            }

            return PropertyData<ulong>.Create(value, propertyInfo.Name);
        }

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
        public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
        {
            return writer.WriteUlongAsync(propertyData.ToTyped<ulong>().Get(), propertyData);
        }
    }

    /// <summary>
    /// Provider for setting and getting data of a <see cref="long"/> type.
    /// </summary>
    internal class LongValueProvider : ValueProviderBase<long>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = long.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
			{
				value = await reader.ReadLongAsync(propertyInfo, value).ConfigureAwait(false);
			}

            return PropertyData<long>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteLongAsync(propertyData.ToTyped<long>().Get(), propertyData);
        }
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="double"/> type.
    /// </summary>
    internal class DoubleValueProvider : ValueProviderBase<double>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = double.NaN;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
			{
				value = await reader.ReadDoubleAsync(propertyInfo, value).ConfigureAwait(false);
			}

            return PropertyData<double>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteDoubleAsync(propertyData.ToTyped<double>().Get(), propertyData);
		}
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="bool"/> type.
    /// </summary>
    internal class BooleanValueProvider : ValueProviderBase<bool>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = false;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
			{
				value = await reader.ReadBooleanAsync(propertyInfo, value).ConfigureAwait(false);
			}
            
            return PropertyData<bool>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
            return writer.WriteBooleanAsync(propertyData.ToTyped<bool>().Get(), propertyData);
		}
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="Enum"/> type.
    /// </summary>
    internal class EnumValueProvider : ValueProviderBase<Enum>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
        {
            var value = (Enum)Enum.ToObject(propertyInfo.Type, -1);

            if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
            {
                var rawValue = await reader.ReadStringAsync(propertyInfo, string.Empty).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    try
                    {
                        value = (Enum)Enum.Parse(propertyInfo.Type, rawValue);
                    }
                    catch
                    {
                        SourceHelper.ThrowCannotConverted(rawValue, propertyInfo.Type);
                    }
                }
			}

            return PropertyData.Create(value, propertyInfo.Name);
        }

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
        {
            return writer.WriteStringAsync(propertyData.Get<Enum>().ToString(), propertyData);
		}
	}

    /// <summary>
    /// Provider for setting and getting data of a type <see cref="ReadOnlyMemory{T}"/> whose generic type argument is <see cref="byte"/>.
    /// </summary>
    internal class BytesValueProvider : ValueProviderBase<ReadOnlyMemory<byte>>
	{
        /// <summary>
        /// Getting a comparer for a supported type.
        /// </summary>
        /// <returns>Comparer.</returns>
        public new static IEqualityComparer GetComparer()
        {
            return new ReadOnlyMemoryByteComparer();
        }

        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = ReadOnlyMemory<byte>.Empty;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
            {
                value = await reader.ReadBytesAsync(propertyInfo, ReadOnlyMemory<byte>.Empty).ConfigureAwait(false);
            }

            return PropertyData<ReadOnlyMemory<byte>>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
            return writer.WriteBytesAsync(propertyData.ToTyped<ReadOnlyMemory<byte>>().Get(), propertyData);
		}

        #region Nested types

        private class ReadOnlyMemoryByteComparer : IEqualityComparer<ReadOnlyMemory<byte>>, IEqualityComparer
        {
            public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
            {
                return x.Length == y.Length && x.IsEmpty == y.IsEmpty && x.Span.SequenceEqual(y.Span);
            }

            public int GetHashCode(ReadOnlyMemory<byte> obj)
            {
                return obj.GetHashCode();
            }

            bool IEqualityComparer.Equals(object? x, object? y)
            {
                if (x is not ReadOnlyMemory<byte> lhs || y is not ReadOnlyMemory<byte> rhs)
                {
                    return false;
                }

                return Equals(lhs, rhs);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }

        #endregion
    }

    /// <summary>
    /// Provider for setting and getting data of a <see cref="decimal"/> type.
    /// </summary>
    internal class DecimalValueProvider : ValueProviderBase<decimal>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = decimal.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
			{
				value = await reader.ReadDecimalAsync(propertyInfo, value).ConfigureAwait(false);
			}
            
            return PropertyData<decimal>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
            return writer.WriteDecimalAsync(propertyData.ToTyped<decimal>().Get(), propertyData);
		}
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="DateTime"/> type.
    /// </summary>
    internal class DateTimeValueProvider : ValueProviderBase<DateTime>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = DateTime.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
            {
                var rawValue = await reader.ReadStringAsync(propertyInfo, string.Empty).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    if (!DateTime.TryParse(rawValue, out value))
                    {
                        SourceHelper.ThrowCannotConverted<DateTime>(rawValue);
                    }
                }
			}
            
            return PropertyData<DateTime>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteStringAsync(propertyData.ToTyped<DateTime>().Get().ToString(CultureInfo.CurrentCulture), propertyData);
		}
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="DateOnly"/> type.
    /// </summary>
    internal class DateOnlyValueProvider : ValueProviderBase<DateOnly>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = DateOnly.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
            {
                var rawValue = await reader.ReadStringAsync(propertyInfo, string.Empty).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    if (!DateOnly.TryParse(rawValue, out value))
                    {
                        SourceHelper.ThrowCannotConverted<DateOnly>(rawValue);
                    }
                }
            }
            
            return PropertyData<DateOnly>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteStringAsync(propertyData.ToTyped<DateOnly>().Get().ToString(CultureInfo.CurrentCulture), propertyData);
		}
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="TimeOnly"/> type.
    /// </summary>
    internal class TimeOnlyValueProvider : ValueProviderBase<TimeOnly>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
        public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = TimeOnly.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
            {
                var rawValue = await reader.ReadStringAsync(propertyInfo, string.Empty).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    if (!TimeOnly.TryParse(rawValue, out value))
                    {
                        SourceHelper.ThrowCannotConverted<TimeOnly>(rawValue);
                    }
                }
            }
            
            return PropertyData<TimeOnly>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteStringAsync(propertyData.ToTyped<TimeOnly>().Get().ToLongTimeString(), propertyData);
        }
	}

    /// <summary>
    /// Provider for setting and getting data of a <see cref="IPEndPoint"/> type.
    /// </summary>
    internal class IpEndPointValueProvider : ValueProviderBase<IPEndPoint>
	{
        /// <summary>
        /// Getting value.
        /// </summary>
        /// <param name="reader">Configuration reader.</param>
        /// <param name="propertyInfo">Property info.</param>
        /// <returns>Value.</returns>
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = new IPEndPoint(IPAddress.None, IPEndPoint.MinPort);

			if (await reader.PropertyExistsAsync(propertyInfo).ConfigureAwait(false))
            {
                var rawValue = await reader.ReadStringAsync(propertyInfo, string.Empty).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    if (!IPEndPoint.TryParse(rawValue, out value))
                    {
                        SourceHelper.ThrowCannotConverted<IPEndPoint>(rawValue);
                    }
                }
            }

            return PropertyData<IPEndPoint>.Create(value, propertyInfo.Name);
		}

        /// <summary>
        /// Setting value.
        /// </summary>
        /// <param name="writer">Configuration writer.</param>
        /// <param name="propertyData">Stored property.</param>
		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			return writer.WriteStringAsync(propertyData.ToTyped<IPEndPoint>().Get().ToString(), propertyData);
		}
	}
}