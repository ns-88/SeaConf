using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SeaConf.Common.Infrastructure;
using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Common.Models;

using ModelProperties = (IMemoryModel Model, Dictionary<string, IPropertyData> Properties);

/// <inheritdoc cref="IChangedModels" />
internal class ChangedModels : IChangedModels, IReadOnlyList<IMemoryModel>, IReadOnlyDictionary<ModelData, IMemoryModel>
{
	private readonly Dictionary<ModelData, ModelProperties> _models = new();

	/// <inheritdoc />
	public bool HasChanged => _models.Count != 0;

	/// <inheritdoc />
	public bool TryGetProperties<T>(string modelName, [MaybeNullWhen(false)] out IReadOnlyCollection<IPropertyData> properties)
	{
		properties = null;

		var key = new ModelData(modelName, typeof(T));

		if (!HasChanged || !_models.TryGetValue(key, out var values))
		{
			return false;
		}

		properties = values.Properties.Values;

		return true;
	}

	/// <inheritdoc />
	public bool CheckProperty<T>(string modelName, string propertyName)
	{
		Guard.ThrowIfEmptyString(propertyName);

		var key = new ModelData(modelName, typeof(T));

		if (!HasChanged || !_models.TryGetValue(key, out var values))
		{
			return false;
		}

		return values.Properties.ContainsKey(propertyName);
	}

	/// <summary>
	/// Adding data about a changed property.
	/// </summary>
	/// <param name="propertyData">Property.</param>
	/// <param name="memoryModel">Model.</param>
	public void Add(IPropertyData propertyData, IMemoryModel memoryModel)
	{
		Guard.ThrowIfNull(propertyData);
		Guard.ThrowIfNull(memoryModel);

		var key = new ModelData(memoryModel.Name, memoryModel.Type);

		if (_models.TryGetValue(key, out var properties))
		{
			properties.Properties.Add(propertyData.Name, propertyData);
		}
		else
		{
			_models.Add(key, new ModelProperties(memoryModel, new Dictionary<string, IPropertyData> { { propertyData.Name, propertyData } }));
		}
	}

	#region Implementation of IEnumerable

	/// <inheritdoc />
	IEnumerator<KeyValuePair<ModelData, IMemoryModel>> IEnumerable<KeyValuePair<ModelData, IMemoryModel>>.GetEnumerator()
	{
		return _models.Select(model => new KeyValuePair<ModelData, IMemoryModel>(model.Key, model.Value.Model)).GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator<IMemoryModel> IEnumerable<IMemoryModel>.GetEnumerator()
	{
		return _models.Select(model => model.Value.Model).GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _models.GetEnumerator();
	}

	#endregion

	#region Implementation of IReadOnlyCollection<out IMemoryModel>

	/// <inheritdoc />
	int IReadOnlyCollection<IMemoryModel>.Count => _models.Count;

	#endregion

	#region Implementation of IReadOnlyList<out IMemoryModel>

	/// <inheritdoc />
	public IMemoryModel this[int index] => _models.ElementAt(index).Value.Model;

	#endregion

	#region Implementation of IReadOnlyCollection<out KeyValuePair<ModelData,IMemoryModel>>

	/// <inheritdoc />
	int IReadOnlyCollection<KeyValuePair<ModelData, IMemoryModel>>.Count => _models.Count;

	#endregion

	#region Implementation of IReadOnlyDictionary<ModelData,IMemoryModel>

	/// <inheritdoc />
	public bool ContainsKey(ModelData key)
	{
		return _models.ContainsKey(key);
	}

	/// <inheritdoc />
	public bool TryGetValue(ModelData key, [MaybeNullWhen(false)] out IMemoryModel value)
	{
		value = null;

		if (!_models.TryGetValue(key, out var modelProperties))
		{
			return false;
		}

		value = modelProperties.Model;

		return true;
	}

	/// <inheritdoc />
	public IMemoryModel this[ModelData key] => _models[key].Model;

	/// <inheritdoc />
	public IEnumerable<ModelData> Keys => _models.Keys;

	/// <inheritdoc />
	public IEnumerable<IMemoryModel> Values => _models.Values.Select(x => x.Model);

	#endregion
}