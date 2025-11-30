using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;

namespace SeaConf.Core.Common.Abstractions;

/// <summary>
/// Configuration components.
/// </summary>
internal interface IComponents
{
	/// <summary>
	/// Registered data models.
	/// </summary>
	IReadOnlyDictionary<ModelData, IModel> RegisteredModels { get; }

	/// <summary>
	/// Known types.
	/// </summary>
	IReadOnlyDictionary<Type, Type> KnownTypes { get; }

	/// <summary>
	/// Raise configuration load event.
	/// </summary>
	void RaiseLoadedEvent();

	/// <summary>
	/// Raise configuration saving event.
	/// </summary>
	/// <param name="changedModels">Modified data models.</param>
	void RaiseSavedEvent(IChangedModels changedModels);

	/// <summary>
	/// Raise roperty change event in data model.
	/// </summary>
	void RaisePropertyChangedEvent(IPropertyData propertyData);

	/// <summary>
	/// Get comparer for comparing supported data types.
	/// </summary>
	/// <typeparam name="T">Supported data type.</typeparam>
	/// <returns>Comparer.</returns>
	IEqualityComparer<T> GetComparer<T>();

	/// <summary>
	/// Throw exception if type is not supported.
	/// </summary>
	/// <param name="type">Type.</param>
	void ThrowIfNotSupportedType(Type type);
}