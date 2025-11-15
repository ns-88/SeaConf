using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Common.Models;

/// <summary>
/// Configuration data model in storage.
/// </summary>
public abstract class StorageModelBase : INode, IStorageModel
{
	/// <summary>
	/// Name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Path.
	/// </summary>
	public ModelPath Path { get; }

	protected StorageModelBase(string name, ModelPath path)
	{
		Name = name;
		Path = path;
	}

	/// <summary>
	/// Getting child elements.
	/// </summary>
	/// <returns>Child elements.</returns>
	public abstract ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync();

	/// <summary>
	/// Creating a writer.
	/// </summary>
	public abstract IWriter CreateWriter();

	/// <summary>
	/// Creating a reader.
	/// </summary>
	public abstract IReader CreateReader();

	/// <summary>
	/// Loading.
	/// </summary>
	public virtual ValueTask LoadAsync()
	{
		return ValueTask.CompletedTask;
	}

	/// <summary>
	/// Saving.
	/// </summary>
	public virtual ValueTask SaveAsync()
	{
		return ValueTask.CompletedTask;
	}

	/// <summary>
	/// Adding property.
	/// </summary>
	/// <param name="propertyInfo">Information about the stored property.</param>
	public abstract ValueTask AddPropertyAsync(IProperty propertyInfo);

	/// <summary>
	/// Deleting property.
	/// </summary>
	/// <param name="propertyInfo">Information about the stored property.</param>
	public abstract ValueTask DeletePropertyAsync(IProperty propertyInfo);

	/// <summary>
	/// Getting all properties.
	/// </summary>
	/// <returns>All properties.</returns>
	public abstract IEnumerable<IProperty> GetProperties();

	/// <inheritdoc />
	public virtual ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}
}