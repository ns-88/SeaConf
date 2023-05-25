using System;
using System.Threading.Tasks;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini.ValueProviders
{
	internal class StringValueProvider : ValueProviderBase<string>
	{
		public StringValueProvider(string collectionName, ISettingsSourceProvider sourceProvider)
			: base(collectionName, sourceProvider)
		{
		}

		public override async ValueTask<IPropertyData> GetAsync(string propertyName)
		{
			var value = string.Empty;

			if (await ReadableStore.PropertyExistsAsync(CollectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetStringValueAsync(CollectionName, propertyName);
			}

			return new PropertyData<string>(value, propertyName);
		}

		public override ValueTask SetAsync(IPropertyData value)
		{
			var typedData = value.ToTyped<string>();

			return WriteableStore.SetStringValueAsync(typedData.Get(), CollectionName, value.Name);
		}
	}

	internal class IntValueProvider : ValueProviderBase<int>
	{
		public IntValueProvider(string collectionName, ISettingsSourceProvider sourceProvider)
			: base(collectionName, sourceProvider)
		{
		}

		public override async ValueTask<IPropertyData> GetAsync(string propertyName)
		{
			var value = int.MinValue;

			if (await ReadableStore.PropertyExistsAsync(CollectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetIntValueAsync(CollectionName, propertyName);
			}

			return new PropertyData<int>(value, propertyName);
		}

		public override ValueTask SetAsync(IPropertyData value)
		{
			var typedData = value.ToTyped<int>();

			return WriteableStore.SetIntValueAsync(typedData.Get(), CollectionName, value.Name);
		}
	}

	internal class LongValueProvider : ValueProviderBase<long>
	{
		public LongValueProvider(string collectionName, ISettingsSourceProvider sourceProvider)
			: base(collectionName, sourceProvider)
		{
		}

		public override async ValueTask<IPropertyData> GetAsync(string propertyName)
		{
			var value = long.MinValue;

			if (await ReadableStore.PropertyExistsAsync(CollectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetLongValueAsync(CollectionName, propertyName);
			}

			return new PropertyData<long>(value, propertyName);
		}

		public override ValueTask SetAsync(IPropertyData value)
		{
			var typedData = value.ToTyped<long>();

			return WriteableStore.SetLongValueAsync(typedData.Get(), CollectionName, value.Name);
		}
	}

	internal class DoubleValueProvider : ValueProviderBase<double>
	{
		public DoubleValueProvider(string collectionName, ISettingsSourceProvider sourceProvider)
			: base(collectionName, sourceProvider)
		{
		}

		public override async ValueTask<IPropertyData> GetAsync(string propertyName)
		{
			var value = double.NaN;

			if (await ReadableStore.PropertyExistsAsync(CollectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetDoubleValueAsync(CollectionName, propertyName);
			}

			return new PropertyData<double>(value, propertyName);
		}

		public override ValueTask SetAsync(IPropertyData value)
		{
			var typedData = value.ToTyped<double>();

			return WriteableStore.SetDoubleValueAsync(typedData.Get(), CollectionName, value.Name);
		}
	}

	internal class BytesValueProvider : ValueProviderBase<double>
	{
		public BytesValueProvider(string collectionName, ISettingsSourceProvider sourceProvider)
			: base(collectionName, sourceProvider)
		{
		}

		public override async ValueTask<IPropertyData> GetAsync(string propertyName)
		{
			var value = ReadOnlyMemory<byte>.Empty;

			if (await ReadableStore.PropertyExistsAsync(CollectionName, propertyName).ConfigureAwait(false))
			{
				value = await ReadableStore.GetBytesValueAsync(CollectionName, propertyName);
			}

			return new PropertyData<ReadOnlyMemory<byte>>(value, propertyName);
		}

		public override ValueTask SetAsync(IPropertyData value)
		{
			var typedData = value.ToTyped<ReadOnlyMemory<byte>>();

			return WriteableStore.SetBytesValueAsync(typedData.Get(), CollectionName, value.Name);
		}
	}
}