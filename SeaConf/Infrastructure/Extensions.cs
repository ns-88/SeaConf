using System;
using System.Collections.Generic;
using SeaConf.Core;
using SeaConf.Interfaces;
using SeaConf.Interfaces.Core;

namespace SeaConf.Infrastructure
{
    internal static class TypeExtensions
	{
		public static bool IsReadOnlyByteMemory(this Type type)
		{
            return type == typeof(ReadOnlyMemory<byte>);
        }
	}

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
}