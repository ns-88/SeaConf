using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini.Interfaces
{
	public interface IRegistryPathBuilder
	{
		ISettingsSourceProviderFactory Path(string appPath);
	}

	public interface IXmlPathBuilder
	{
		ISettingsSourceProviderFactory Path(string appPath);
		ISettingsSourceProviderFactory LocalAppDataPath(string companyName, string appName);
	}
}