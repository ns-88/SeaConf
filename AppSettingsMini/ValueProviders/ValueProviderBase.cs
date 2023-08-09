using System;
using System.Threading.Tasks;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.ValueProviders;

namespace AppSettingsMini.ValueProviders
{
	public abstract class ValueProviderBase<T> : IValueProvider
	{
		protected readonly IReadableSettingsSource ReadableStore;
		protected readonly IWriteableSettingsSource WriteableStore;

		public Type Type => typeof(T);

		protected ValueProviderBase(ISettingsSourceProvider sourceProvider)
		{
			ReadableStore = sourceProvider.ReadableSettingsSource;
			WriteableStore = sourceProvider.WriteableSettingsSource;
		}

		public abstract ValueTask<ISettingsPropertyData> GetAsync(string collectionName, string propertyName);

		public abstract ValueTask SetAsync(string collectionName, ISettingsPropertyData propertyData);
	}
}