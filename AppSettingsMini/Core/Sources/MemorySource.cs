using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppSettingsMini.Interfaces;
using AppSettingsMini.Interfaces.Core;
using AppSettingsMini.Models;

namespace AppSettingsMini.Core.Sources
{
	internal class MemorySource : SourceBase<IMemoryModel>
	{
		private readonly IEnumerable<IModelInfo> _models;

		public MemorySource(IEnumerable<IModelInfo> models)
		{
			_models = models;
		}

		public override ValueTask<IReadOnlyList<INode>> GetRootNodes()
		{
			var nodes = new List<INode>();

			foreach (var model in _models)
			{
				if (!model.IsRoot)
				{
					continue;
				}

				nodes.Add(new MemoryNode(model));
			}

			return ValueTask.FromResult((IReadOnlyList<INode>)nodes);
		}
	}

	file class MemoryNode : INode, IMemoryModel, IPathModel
	{
		private readonly IModelInfo _modelInfo;

		public string Name { get; }
		public Type Type { get; }
		public ModelPath Path { get; }

		private MemoryNode(IModelInfo modelInfo, ModelPath path)
		{
			_modelInfo = modelInfo;

			Name = modelInfo.Name;
			Type = modelInfo.Type;
			Path = new ModelPath(Name, path);
		}

		public MemoryNode(IModelInfo modelInfo)
		{
			_modelInfo = modelInfo;

			Name = modelInfo.Name;
			Type = modelInfo.Type;
			Path = new ModelPath(modelInfo.Name);
		}

		public ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
		{
			var nodes = new List<INode>();

			foreach (var modelInfo in _modelInfo.InnerModels)
			{
				nodes.Add(new MemoryNode(modelInfo, Path));
			}

			return ValueTask.FromResult((IReadOnlyList<INode>)nodes);
		}

		public IEnumerable<IPropertyData> GetModifiedProperties()
		{
			return _modelInfo.Model.GetModifiedProperties();
		}

		public IReadOnlyCollection<IPropertyData> GetPropertiesData()
		{
			return _modelInfo.Model.GetPropertiesData();
		}

		public override string ToString()
		{
			return $"Name = {Name}, Type = {_modelInfo.Type.Name}";
		}
	}
}