using Moq;
using SeaConf.Core.Common;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;

namespace SeaConf.Test.Infrastructure;

internal class SynchronizedStorageSource : SourceBase<IStorageModel>, IStorageSource
{
	private readonly Dictionary<ModelPath, IStorageModel> _models;

	public SynchronizedStorageSource()
	{
		_models = new Dictionary<ModelPath, IStorageModel>();
	}

	/// <inheritdoc />
	public override ValueTask<IReadOnlyList<INode>> GetRootNodesAsync()
	{
		return ValueTask.FromResult<IReadOnlyList<INode>>(_models
			.Where(x => x.Value.Path.Count == 1)
			.Select(x => (INode)x.Value)
			.ToList()
		);
	}

	/// <inheritdoc />
	public ValueTask LoadAsync()
	{
		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public ValueTask SaveAsync()
	{
		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public ValueTask<IStorageModel> AddModelAsync(ModelPath path)
	{
		var model = new SynchronizedStorageModel(path[^1], path, _models);

		_models.Add(path, model);

		return ValueTask.FromResult<IStorageModel>(model);
	}

	/// <inheritdoc />
	public ValueTask DeleteModelAsync(ModelPath path)
	{
		_models.Remove(path);
		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}

	#region Nested types

	private class SynchronizedStorageModel : StorageModelBase
	{
		private readonly IWriter _writer;
		private readonly IReader _reader;
		private readonly IReadOnlyDictionary<ModelPath, IStorageModel> _models;
		private readonly List<IProperty> _properties;

		public SynchronizedStorageModel(string name, ModelPath path, IReadOnlyDictionary<ModelPath, IStorageModel> models) : base(name, path)
		{
			_writer = Mock.Of<IWriter>();
			_reader = Mock.Of<IReader>();
			_models = models;
			_properties = new List<IProperty>();
		}

		/// <inheritdoc />
		public override ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
		{
			var nodes = new List<INode>();

			foreach (var model in _models)
			{
				if (model.Value.Path.IsIncluded(Path, limit: 1))
				{
					nodes.Add((INode)model.Value);
				}
			}

			return ValueTask.FromResult<IReadOnlyList<INode>>(nodes);
		}

		/// <inheritdoc />
		public override ValueTask AddPropertyAsync(IProperty propertyInfo)
		{
			_properties.Add(propertyInfo);
			return ValueTask.CompletedTask;
		}

		/// <inheritdoc />
		public override ValueTask DeletePropertyAsync(IProperty propertyInfo)
		{
			return ValueTask.CompletedTask;
		}

		/// <inheritdoc />
		public override IEnumerable<IProperty> GetProperties()
		{
			return _properties;
		}

		/// <inheritdoc />
		public override IWriter CreateWriter()
		{
			return _writer;
		}

		/// <inheritdoc />
		public override IReader CreateReader()
		{
			return _reader;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Name = {Name}";
		}
	}

	#endregion
}