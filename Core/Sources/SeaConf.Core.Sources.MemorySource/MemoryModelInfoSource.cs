using SeaConf.Core.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;

namespace SeaConf.Core.Sources.MemorySource;

internal sealed class MemoryModelInfoSource : SourceBase<MemoryModelInfo>
{
	private readonly IReadOnlyDictionary<ModelData, IModel> _registeredModels;
	private readonly IComponents _components;

	public MemoryModelInfoSource(IComponents components)
	{
		_registeredModels = components.RegisteredModels;
		_components = components;
	}

	/// <inheritdoc />
	public override ValueTask<IReadOnlyList<INode>> GetRootNodesAsync()
	{
		var nodes = new List<INode>();

		foreach (var (modelData, model) in _registeredModels)
		{
			nodes.Add(new MemoryModelInfo(_components)
			{
				Model = (IMemoryModel)model,
				Name = modelData.Name,
				Type = modelData.Type,
				Path = new ModelPath(modelData.Name)
			});
		}

		return ValueTask.FromResult((IReadOnlyList<INode>)nodes);
	}
}