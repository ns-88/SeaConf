using System.Collections;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;
using System.Diagnostics.CodeAnalysis;

namespace SeaConf.Configurations;

internal partial class Configuration : IReadOnlyDictionary<ModelData, IMemoryModel>, IReadOnlyList<IMemoryModel>
{
	#region Implementation of IEnumerable

	/// <inheritdoc />
	IEnumerator<KeyValuePair<ModelData, IMemoryModel>> IEnumerable<KeyValuePair<ModelData, IMemoryModel>>.GetEnumerator()
	{
		return _memorySource.Models.GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator<IMemoryModel> IEnumerable<IMemoryModel>.GetEnumerator()
	{
		return _memorySource.Models.Values.GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return _memorySource.Models.GetEnumerator();
	}

	#endregion

	#region Implementation of IReadOnlyCollection<out IMemoryModel>

	/// <inheritdoc />
	int IReadOnlyCollection<IMemoryModel>.Count => _memorySource.Models.Count;

	#endregion

	#region Implementation of IReadOnlyList<out IMemoryModel>

	/// <inheritdoc />
	public IMemoryModel this[int index] => _memorySource.Models.Values.ElementAt(index);

	#endregion

	#region Implementation of IReadOnlyCollection<out KeyValuePair<string,IMemoryModel>>

	/// <inheritdoc />
	int IReadOnlyCollection<KeyValuePair<ModelData, IMemoryModel>>.Count => _memorySource.Models.Count;

	#endregion

	#region Implementation of IReadOnlyDictionary<string,IMemoryModel>

	/// <inheritdoc />
	public bool ContainsKey(ModelData key)
	{
		return _memorySource.Models.ContainsKey(key);
	}

	/// <inheritdoc />
	public bool TryGetValue(ModelData key, [MaybeNullWhen(false)] out IMemoryModel value)
	{
		return _memorySource.Models.TryGetValue(key, out value);
	}

	/// <inheritdoc />
	public IMemoryModel this[ModelData key] => _memorySource.Models[key];

	/// <inheritdoc />
	public IEnumerable<ModelData> Keys => _memorySource.Models.Keys;

	/// <inheritdoc />
	public IEnumerable<IMemoryModel> Values => _memorySource.Models.Values;

	#endregion
}