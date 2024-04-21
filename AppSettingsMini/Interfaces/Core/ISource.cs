using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppSettingsMini.Models;

namespace AppSettingsMini.Interfaces.Core
{
	internal interface IWritableSource<out TModel>
		where TModel : class, IModel
	{
		TModel AddModel(ModelPath path);
		void DeleteModel(ModelPath path);
	}

	public interface ISource<out TModel> : IAsyncDisposable
		where TModel : class, IModel
	{
		ValueTask<IReadOnlyList<INode>> GetRootNodes();
		IAsyncEnumerable<TModel> GetModelsAsync(IEnumerable<INode> rootNodes);
		ValueTask LoadAsync();
		ValueTask SaveAsync();
	}
}