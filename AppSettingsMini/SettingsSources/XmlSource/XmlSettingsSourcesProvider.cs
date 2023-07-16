namespace AppSettingsMini.SettingsSources.XmlSource
{
	internal class XmlSettingsSourcesProvider : SettingsSourceProviderBase
    {
	    public XmlSettingsSourcesProvider(string path, string rootElementName)
	        : base (new XmlSettingsSource(path, rootElementName))
        {
            
        }
    }
}