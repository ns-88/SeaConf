using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Interfaces.Factories;

namespace AppSettingsMini.Interfaces
{
	public interface IRegistryPathBuilder
	{
		ISourceFactory<IStorageModel> Path(string companyName, string appName);
	}

	public interface IXmlPathBuilder
	{
		ISourceFactory<IStorageModel> Path(string appPath);
		ISourceFactory<IStorageModel> LocalAppDataPath(string companyName, string appName);
		ISourceFactory<IStorageModel> ExecutablePath();
	}
}