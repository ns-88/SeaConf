using System.Threading.Tasks;
using AppSettingsMini.Interfaces;

namespace AppSettingsMini.SettingsSources
{
	public class SettingsSourceProviderBase : ISettingsSourceProvider
	{
		private readonly ISettingsSource _settingsSource;

		public IReadableSettingsSource ReadableSettingsSource { get; }
		public IWriteableSettingsSource WriteableSettingsSource { get; }

		protected SettingsSourceProviderBase(ISettingsSource source)
		{
			_settingsSource = source;

			ReadableSettingsSource = (IReadableSettingsSource)source;
			WriteableSettingsSource = (IWriteableSettingsSource)source;
		}

		public ValueTask LoadAsync()
		{
			return _settingsSource.LoadAsync();
		}

		public ValueTask SaveAsync()
		{
			return _settingsSource.SaveAsync();
		}

		public void Dispose()
		{
			_settingsSource.Dispose();
		}

		public ValueTask DisposeAsync()
		{
			return _settingsSource.DisposeAsync();
		}
	}
}