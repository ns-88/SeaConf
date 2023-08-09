using System;
using System.Threading.Tasks;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini.ValueProviders
{
	internal class StringValueProvider : ValueProviderBase<string>
	{
		public StringValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, string propertyName)
		{
			var value = string.Empty;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetStringValueAsync(collectionName, propertyName).ConfigureAwait(false);
			}

			return new SettingsPropertyData<string>(value, propertyName);
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

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, string propertyName)
		{
			var value = int.MinValue;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetIntValueAsync(collectionName, propertyName).ConfigureAwait(false);
			}

			return new SettingsPropertyData<int>(value, propertyName);
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

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, string propertyName)
		{
			var value = long.MinValue;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetLongValueAsync(collectionName, propertyName).ConfigureAwait(false);
			}

			return new SettingsPropertyData<long>(value, propertyName);
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

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, string propertyName)
		{
			var value = double.NaN;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetDoubleValueAsync(collectionName, propertyName).ConfigureAwait(false);
			}

			return new SettingsPropertyData<double>(value, propertyName);
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<double>();

			return WriteableStore.SetDoubleValueAsync(typedData.Get(), collectionName, propertyData.Name);
		}
	}

	internal class BytesValueProvider : ValueProviderBase<double>
	{
		public BytesValueProvider(ISettingsSourceProvider sourceProvider)
			: base(sourceProvider)
		{
		}

		public override async ValueTask<ISettingsPropertyData> GetAsync(string collectionName, string propertyName)
		{
			var value = ReadOnlyMemory<byte>.Empty;

			if (await ReadableStore.PropertyExistsAsync(collectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetBytesValueAsync(collectionName, propertyName).ConfigureAwait(false);
			}

			return new SettingsPropertyData<ReadOnlyMemory<byte>>(value, propertyName);
		}

		public override ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData)
		{
			var typedData = propertyData.ToTyped<ReadOnlyMemory<byte>>();

			return WriteableStore.SetBytesValueAsync(typedData.Get(), collectionName, propertyData.Name);
		}
	}
}