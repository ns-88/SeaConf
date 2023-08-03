using System.Collections.Generic;

namespace AppSettingsMini.Interfaces
{
	public interface ISettingsModel
	{
		internal void Init(SettingsServiceBase service);
		internal IEnumerable<ISettingsPropertyData> GetModifiedProperties();
		internal IReadOnlyDictionary<string, ISettingsPropertyData> GetPropertiesData();
	}
}