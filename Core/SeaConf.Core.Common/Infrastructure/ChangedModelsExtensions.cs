using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;

namespace SeaConf.Core.Common.Infrastructure;

public static class ChangedModelsExtensions
{
	public static IReadOnlyList<IMemoryModel> AsList(this IChangedModels changedModels)
	{
		return (IReadOnlyList<IMemoryModel>)changedModels;
	}

	public static IReadOnlyDictionary<ModelData, IMemoryModel> AsDictionary(this IChangedModels changedModels)
	{
		return (IReadOnlyDictionary<ModelData, IMemoryModel>)changedModels;
	}
}