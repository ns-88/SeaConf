using System;
using System.Threading.Tasks;

namespace AppSettingsMini.SettingsSources
{
	public class SettingsSourceBase : IDisposable, IAsyncDisposable
	{
		public virtual ValueTask LoadAsync()
		{
			return new ValueTask();
		}

		public virtual ValueTask SaveAsync()
		{
			return new ValueTask();
		}

		public virtual void Dispose()
		{
		}

		public virtual ValueTask DisposeAsync()
		{
			return new ValueTask();
		}
	}
}