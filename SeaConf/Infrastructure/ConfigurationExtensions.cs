using SeaConf.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;

namespace SeaConf.Infrastructure;

public static class ConfigurationExtensions
{
	public static IReadOnlyList<IMemoryModel> AsList(this IConfiguration configuration)
	{
		return (IReadOnlyList<IMemoryModel>)configuration;
	}

	public static IReadOnlyDictionary<ModelData, IMemoryModel> AsDictionary(this IConfiguration configuration)
	{
		return (IReadOnlyDictionary<ModelData, IMemoryModel>)configuration;
	}
}