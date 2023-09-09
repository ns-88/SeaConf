using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini.Interfaces
{
	public interface IRegistryPathBuilder
	{
		ISettingsSourceProviderFactory Path(string companyName, string appName);
	}

	public interface IXmlPathBuilder
	{
		ISettingsSourceProviderFactory Path(string appPath);
		ISettingsSourceProviderFactory LocalAppDataPath(string companyName, string appName);
		ISettingsSourceProviderFactory ExecutablePath();
	}
}