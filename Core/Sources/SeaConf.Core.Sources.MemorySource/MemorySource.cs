using SeaConf.Common;
using SeaConf.Common.Infrastructure;
using SeaConf.Core.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;

namespace SeaConf.Core.Sources.MemorySource;

/// <summary>
/// Configuration data source in memory.
/// </summary>
internal sealed class MemorySource : SourceBase<IMemoryModel>, IMemorySource
{
	private IReadOnlyList<INode>? _rootNodes;

	/// <summary>
	/// Configuration data models.
	/// </summary>
	public IReadOnlyDictionary<ModelData, IMemoryModel> Models { get; }

	public MemorySource()
	{
		Models = new Dictionary<ModelData, IMemoryModel>();
	}

	/// <summary>
	/// Getting data model.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <param name="name">Name</param>
	/// <returns>Data model.</returns>
	public T GetModel<T>(string? name) where T : class
	{
		if (name != null && name.Trim().Length == 0)
		{
			throw new ArgumentException(nameof(name));
		}

		var modelType = typeof(T);
		var modelName = name ?? IMemoryModel.GetName(modelType);

		if (!Models.TryGetValue(new ModelData(modelName, modelType), out var model))
		{
			throw new InvalidOperationException(string.Format(Resources.ModelNotRegistered, modelType, modelName));
		}

		return (T)model;
	}

	/// <summary>
	/// Initializing.
	/// </summary>
	/// <param name="components">Configuration components.</param>
	public void Initialize(IComponents components)
	{
		var source = new MemoryModelInfoSource(components);
		var rootNodes = source.GetRootNodesAsync().Result;
		var infoModels = source.GetModelsAsync(rootNodes).ToBlockingEnumerable();
		var models = (Dictionary<ModelData, IMemoryModel>)Models;

		foreach (var modelInfo in infoModels)
		{
			var key = new ModelData(modelInfo.Name, modelInfo.Type);

			if (Models.ContainsKey(key))
			{
				throw new InvalidOperationException(string.Format(Resources.ModelAlreadyAdded, key.Name, key.Type));
			}

			try
			{
				((IMemoryInitializedModel)modelInfo.Model).Initialize(modelInfo, modelInfo.PropertiesData);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(string.Format(Resources.ModelInitializationFailed, modelInfo.Type.Name), ex);
			}

			models.Add(key, modelInfo.Model);
		}

		_rootNodes = rootNodes;
	}

	/// <inheritdoc />
	public override ValueTask<IReadOnlyList<INode>> GetRootNodesAsync()
	{
		Guard.ThrowIfNull(_rootNodes);
		return ValueTask.FromResult(_rootNodes!);
	}
}