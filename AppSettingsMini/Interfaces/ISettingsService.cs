using System.Threading.Tasks;

namespace AppSettingsMini.Interfaces
{
	public interface ISettingsService
	{
		ValueTask LoadAsync();
		ValueTask SaveAsync();
		T GetModel<T>();
	}
}