using System;
using System.Threading.Tasks;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.ValueProviders;

namespace AppSettingsMini.ValueProviders
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