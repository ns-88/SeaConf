﻿using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using AppSettingsMini.Core.Factories;
using AppSettingsMini.Core.Sources;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Models;

namespace AppSettingsMini.Core.ValueProviders
{
	public abstract class ValueProviderBase : IValueProvider
	{
		public abstract Type Type { get; }
		public abstract ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo);
		public abstract ValueTask SetAsync(IWriter writer, IPropertyData propertyData);

		internal static bool IsSupportedType(Type type)
		{
			return type == typeof(string) ||
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
			       type.IsReadOnlyByteMemory();
		}

		internal static Type GetKeyType(Type type)
		{
			var keyType = type;

			if (type.IsEnum)
			{
				keyType = typeof(Enum);
			}

			return keyType;
		}

		internal static IValueProvider? Create(Type type, out Type keyType)
		{
			IValueProvider? valueProvider = null;

			keyType = type;

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
				keyType = typeof(Enum);
			}

			return valueProvider;
		}
	}

	public abstract class ValueProviderBase<T> : ValueProviderBase
	{
		public override Type Type { get; } = typeof(T);
	}

	internal class StringValueProvider : ValueProviderBase<string>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = string.Empty;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				value = await reader.ReadStringAsync(propertyInfo.Name).ConfigureAwait(false);
			}

			return new PropertyData<string>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<string>();

			return writer.WriteStringAsync(typedData.Get(), propertyData.Name);
		}
	}

	internal class IntValueProvider : ValueProviderBase<int>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = int.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				value = await reader.ReadIntAsync(propertyInfo.Name).ConfigureAwait(false);
			}

			return new PropertyData<int>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<int>();

			return writer.WriteIntAsync(typedData.Get(), propertyData.Name);
		}
	}

	internal class LongValueProvider : ValueProviderBase<long>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = long.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				value = await reader.ReadLongAsync(propertyInfo.Name).ConfigureAwait(false);
			}

			return new PropertyData<long>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<long>();

			return writer.WriteLongAsync(typedData.Get(), propertyData.Name);
		}
	}

	internal class DoubleValueProvider : ValueProviderBase<double>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = double.NaN;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				value = await reader.ReadDoubleAsync(propertyInfo.Name).ConfigureAwait(false);
			}

			return new PropertyData<double>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<double>();

			return writer.WriteDoubleAsync(typedData.Get(), propertyData.Name);
		}
	}

	internal class BooleanValueProvider : ValueProviderBase<bool>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = false;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				value = await reader.ReadBooleanAsync(propertyInfo.Name).ConfigureAwait(false);
			}

			return new PropertyData<bool>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<bool>();

			return writer.WriteBooleanAsync(typedData.Get(), propertyData.Name);
		}
	}

	internal class EnumValueProvider : ValueProviderBase<Enum>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			Enum value;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				var rawValue = await reader.ReadStringAsync(propertyInfo.Name).ConfigureAwait(false);

				try
				{
					value = (Enum)Enum.Parse(propertyInfo.Type, rawValue);
				}
				catch
				{
					throw new InvalidOperationException(string.Format(Strings.StringValueCannotConvertedToType, propertyInfo.Type, rawValue));
				}
			}
			else
			{
				value = (Enum)Enum.ToObject(propertyInfo.Type, -1);
			}

			return PropertyDataFactory.Create(new ArgsInfo(value, propertyInfo.Name));
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped(propertyData.Type);

			return writer.WriteStringAsync(typedData.Get<Enum>().ToString(), propertyData.Name);
		}
	}

	internal class BytesValueProvider : ValueProviderBase<double>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = ReadOnlyMemory<byte>.Empty;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				value = await reader.ReadBytesAsync(propertyInfo.Name).ConfigureAwait(false);
			}

			return new PropertyData<ReadOnlyMemory<byte>>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<ReadOnlyMemory<byte>>();

			return writer.WriteBytesAsync(typedData.Get(), propertyData.Name);
		}
	}

	internal class DecimalValueProvider : ValueProviderBase<decimal>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = decimal.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				value = await reader.ReadDecimalAsync(propertyInfo.Name).ConfigureAwait(false);
			}

			return new PropertyData<decimal>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<decimal>();

			return writer.WriteDecimalAsync(typedData.Get(), propertyData.Name);
		}
	}

	internal class DateTimeValueProvider : ValueProviderBase<DateTime>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = DateTime.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				var rawValue = await reader.ReadStringAsync(propertyInfo.Name).ConfigureAwait(false);

				if (!DateTime.TryParse(rawValue, out value))
				{
					SourceHelper.ThrowCannotConverted<DateTime>(rawValue);
				}
			}

			return new PropertyData<DateTime>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<DateTime>();

			return writer.WriteStringAsync(typedData.Get().ToString(CultureInfo.CurrentCulture), propertyData.Name);
		}
	}

	internal class DateOnlyValueProvider : ValueProviderBase<DateOnly>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = DateOnly.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				var rawValue = await reader.ReadStringAsync(propertyInfo.Name).ConfigureAwait(false);

				if (!DateOnly.TryParse(rawValue, out value))
				{
					SourceHelper.ThrowCannotConverted<DateOnly>(rawValue);
				}
			}

			return new PropertyData<DateOnly>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<DateOnly>();

			return writer.WriteStringAsync(typedData.Get().ToString(CultureInfo.CurrentCulture), propertyData.Name);
		}
	}

	internal class TimeOnlyValueProvider : ValueProviderBase<TimeOnly>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = TimeOnly.MinValue;

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				var rawValue = await reader.ReadStringAsync(propertyInfo.Name).ConfigureAwait(false);

				if (!TimeOnly.TryParse(rawValue, out value))
				{
					SourceHelper.ThrowCannotConverted<TimeOnly>(rawValue);
				}
			}

			return new PropertyData<TimeOnly>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<TimeOnly>();

			return writer.WriteStringAsync(typedData.Get().ToLongTimeString(), propertyData.Name);
		}
	}

	internal class IpEndPointValueProvider : ValueProviderBase<IPEndPoint>
	{
		public override async ValueTask<IPropertyData> GetAsync(IReader reader, IPropertyInfo propertyInfo)
		{
			var value = new IPEndPoint(IPAddress.None, IPEndPoint.MinPort);

			if (await reader.PropertyExistsAsync(propertyInfo.Name).ConfigureAwait(false))
			{
				var rawValue = await reader.ReadStringAsync(propertyInfo.Name).ConfigureAwait(false);

				if (!IPEndPoint.TryParse(rawValue, out value))
				{
					SourceHelper.ThrowCannotConverted<IPEndPoint>(rawValue);
				}
			}

			return new PropertyData<IPEndPoint>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(IWriter writer, IPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<IPEndPoint>();

			return writer.WriteStringAsync(typedData.Get().ToString(), propertyData.Name);
		}
	}
}