using System;
using System.Threading.Tasks;
using AppSettingsMini.Factories;
using AppSettingsMini.Infrastructure;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Models;

namespace AppSettingsMini.ValueProviders
{
	internal class StringValueProvider : ValueProviderBase<string>
	{
		public StringValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, ISettingsPropertyInfo propertyInfo)
		{
			var value = string.Empty;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyInfo.Name).ConfigureAwait(false))
			{
				value = await ReadableStore.GetStringValueAsync(collectionName, propertyInfo.Name).ConfigureAwait(false);
			}

			return new SettingsPropertyData<string>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<string>();

			return WriteableStore.SetStringValueAsync(typedData.Get(), collectionName, propertyData.Name);
		}
	}

	internal class IntValueProvider : ValueProviderBase<int>
	{
		public IntValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, ISettingsPropertyInfo propertyInfo)
		{
			var value = int.MinValue;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyInfo.Name).ConfigureAwait(false))
			{
				value = await ReadableStore.GetIntValueAsync(collectionName, propertyInfo.Name).ConfigureAwait(false);
			}

			return new SettingsPropertyData<int>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<int>();

			return WriteableStore.SetIntValueAsync(typedData.Get(), collectionName, propertyData.Name);
		}
	}

	internal class LongValueProvider : ValueProviderBase<long>
	{
		public LongValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, ISettingsPropertyInfo propertyInfo)
		{
			var value = long.MinValue;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyInfo.Name).ConfigureAwait(false))
			{
				value = await ReadableStore.GetLongValueAsync(collectionName, propertyInfo.Name).ConfigureAwait(false);
			}

			return new SettingsPropertyData<long>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<long>();

			return WriteableStore.SetLongValueAsync(typedData.Get(), collectionName, propertyData.Name);
		}
	}

	internal class DoubleValueProvider : ValueProviderBase<double>
	{
		public DoubleValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, ISettingsPropertyInfo propertyInfo)
		{
			var value = double.NaN;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyInfo.Name).ConfigureAwait(false))
			{
				value = await ReadableStore.GetDoubleValueAsync(collectionName, propertyInfo.Name).ConfigureAwait(false);
			}

			return new SettingsPropertyData<double>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<double>();

			return WriteableStore.SetDoubleValueAsync(typedData.Get(), collectionName, propertyData.Name);
		}
	}

	internal class BooleanValueProvider : ValueProviderBase<bool>
	{
		public BooleanValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, ISettingsPropertyInfo propertyInfo)
		{
			var value = false;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyInfo.Name).ConfigureAwait(false))
			{
				value = await ReadableStore.GetBooleanValueAsync(collectionName, propertyInfo.Name).ConfigureAwait(false);
			}

			return new SettingsPropertyData<bool>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<bool>();

			return WriteableStore.SetBooleanValueAsync(typedData.Get(), collectionName, propertyData.Name);
		}
	}

	internal class EnumValueProvider : ValueProviderBase<Enum>
	{
		public EnumValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, ISettingsPropertyInfo propertyInfo)
		{
			Enum value;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyInfo.Name).ConfigureAwait(false))
			{
				var rawValue = await ReadableStore.GetStringValueAsync(collectionName, propertyInfo.Name).ConfigureAwait(false);

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

			return SettingsPropertyDataFactory.Create(new ArgsInfo(value, propertyInfo.Name));
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped(propertyData.Type);
			
			return WriteableStore.SetStringValueAsync(typedData.Get<Enum>().ToString(), collectionName, propertyData.Name);
		}
	}

	internal class BytesValueProvider : ValueProviderBase<double>
	{
		public BytesValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, ISettingsPropertyInfo propertyInfo)
		{
			var value = ReadOnlyMemory<byte>.Empty;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyInfo.Name).ConfigureAwait(false))
			{
				value = await ReadableStore.GetBytesValueAsync(collectionName, propertyInfo.Name).ConfigureAwait(false);
			}

			return new SettingsPropertyData<ReadOnlyMemory<byte>>(value, propertyInfo.Name);
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<ReadOnlyMemory<byte>>();

			return WriteableStore.SetBytesValueAsync(typedData.Get(), collectionName, propertyData.Name);
		}
	}
}