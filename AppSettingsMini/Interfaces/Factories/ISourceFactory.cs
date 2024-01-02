using AppSettingsMini.Interfaces.Core;

namespace AppSettingsMini.Interfaces.Factories
{
	public interface ISourceFactory<out TModel> where TModel : class, IModel
	{
		ISource<TModel> Create();
	}
}