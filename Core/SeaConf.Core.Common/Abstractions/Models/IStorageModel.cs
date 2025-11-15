namespace SeaConf.Core.Common.Abstractions.Models;

/// <summary>
/// Configuration data model in storage.
/// </summary>
public interface IStorageModel : IModel, IAsyncDisposable
{
	/// <summary>
	/// Loading.
	/// </summary>
	ValueTask LoadAsync();

	/// <summary>
	/// Saving.
	/// </summary>
	ValueTask SaveAsync();

	/// <summary>
	/// Adding property.
	/// </summary>
	/// <param name="property">Information about the stored property.</param>
	ValueTask AddPropertyAsync(IProperty property);

	/// <summary>
	/// Deleting property.
	/// </summary>
	/// <param name="property">Information about the stored property.</param>
	ValueTask DeletePropertyAsync(IProperty property);

	/// <summary>
	/// Getting all properties.
	/// </summary>
	/// <returns>All properties.</returns>
	IEnumerable<IProperty> GetProperties();

	/// <summary>
	/// Creating a writer.
	/// </summary>
	IWriter CreateWriter();

	/// <summary>
	/// Creating a reader.
	/// </summary>
	IReader CreateReader();
}