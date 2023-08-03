using System;
using System.Threading.Tasks;
using AppSettingsMini.Models;

namespace AppSettingsMini.Interfaces
{
    public interface ISettingsService
	{
		event EventHandler Loaded;
		event EventHandler<IChangedModels> Saved;
		event EventHandler<PropertyChangedEventArgs> PropertyChanged;
		ValueTask LoadAsync();
		ValueTask SaveAsync();
		T GetModel<T>();
	}
}