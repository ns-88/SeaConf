namespace AppSettingsMini.Interfaces
{
	public interface ISettingsModel
	{
		internal IEnumerable<IPropertyData> GetModifiedProperties();
		internal IReadOnlyDictionary<string, IPropertyData> GetPropertiesData();
	}
}