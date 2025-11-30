using System.Collections;
using SeaConf.Core.Common.Abstractions.Models;
using System.Diagnostics.CodeAnalysis;

namespace SeaConf.Core.Common.Models;

public partial class PropertiesModel : IReadOnlyList<IPropertyData>, IReadOnlyDictionary<string, IPropertyData>
{
	#region Implementation of IEnumerable

	/// <inheritdoc />
	IEnumerator<KeyValuePair<string, IPropertyData>> IEnumerable<KeyValuePair<string, IPropertyData>>.GetEnumerator()
	{
		ThrowIfNoInit();
		return _propertiesData.GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator<IPropertyData> IEnumerable<IPropertyData>.GetEnumerator()
	{
		ThrowIfNoInit();
		return _propertiesData.Values.GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		ThrowIfNoInit();
		return _propertiesData.GetEnumerator();
	}

	#endregion

	#region Implementation of IReadOnlyCollection<out IPropertyData>

	/// <inheritdoc />
	int IReadOnlyCollection<IPropertyData>.Count
	{
		get
		{
			ThrowIfNoInit();
			return _propertiesData.Count;
		}
	}

	#endregion

	#region Implementation of IReadOnlyList<out IPropertyData>

	/// <inheritdoc />
	IPropertyData IReadOnlyList<IPropertyData>.this[int index]
	{
		get
		{
			ThrowIfNoInit();
			return _propertiesData.Values.ElementAt(index);
		}
	}

	#endregion

	#region Implementation of IReadOnlyCollection<out KeyValuePair<string,IPropertyData>>

	/// <inheritdoc />
	int IReadOnlyCollection<KeyValuePair<string, IPropertyData>>.Count
	{
		get
		{
			ThrowIfNoInit();
			return _propertiesData.Count;
		}
	}

	#endregion

	#region Implementation of IReadOnlyDictionary<string,IPropertyData>

	/// <inheritdoc />
	bool IReadOnlyDictionary<string, IPropertyData>.ContainsKey(string key)
	{
		ThrowIfNoInit();
		return _propertiesData.ContainsKey(key);
	}

	/// <inheritdoc />
	bool IReadOnlyDictionary<string, IPropertyData>.TryGetValue(string key, [MaybeNullWhen(false)] out IPropertyData value)
	{
		ThrowIfNoInit();
		return _propertiesData.TryGetValue(key, out value);
	}

	/// <inheritdoc />
	IPropertyData IReadOnlyDictionary<string, IPropertyData>.this[string key]
	{
		get
		{
			ThrowIfNoInit();
			return _propertiesData[key];
		}
	}

	/// <inheritdoc />
	IEnumerable<string> IReadOnlyDictionary<string, IPropertyData>.Keys
	{
		get
		{
			ThrowIfNoInit();
			return _propertiesData.Keys;
		}
	}

	/// <inheritdoc />
	IEnumerable<IPropertyData> IReadOnlyDictionary<string, IPropertyData>.Values
	{
		get
		{
			ThrowIfNoInit();
			return _propertiesData.Values;
		}
	}

	#endregion
}