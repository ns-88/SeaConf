using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Common.Infrastructure;

public static class ModelExtensions
{
	public static IReadOnlyList<IPropertyData> AsList(this IMemoryModel model)
	{
		return (IReadOnlyList<IPropertyData>)model;
	}

	public static IReadOnlyDictionary<string, IPropertyData> AsDictionary(this IMemoryModel model)
	{
		return (IReadOnlyDictionary<string, IPropertyData>)model;
	}
}