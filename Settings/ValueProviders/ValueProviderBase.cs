﻿using Settings.Interfaces;
using Settings.Interfaces.ValueProviders;

namespace Settings.ValueProviders
{
	public abstract class ValueProviderBase<T> : IValueProvider
	{
		protected readonly string CollectionName;
		protected readonly IReadableSettingsSource ReadableStore;
		protected readonly IWriteableSettingsSource WriteableStore;

		public Type Type => typeof(T);

		protected ValueProviderBase(string collectionName, ISettingsSourceProvider sourceProvider)
		{
			CollectionName = collectionName;
			ReadableStore = sourceProvider.ReadableSettingsStore;
			WriteableStore = sourceProvider.WriteableSettingsStore;
		}
		public abstract ValueTask<IPropertyData> GetAsync(string propertyName);

		public abstract ValueTask SetAsync(IPropertyData value);
	}
}