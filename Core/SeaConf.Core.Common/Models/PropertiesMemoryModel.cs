using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Common.Models;

public partial class PropertiesModel : IMemoryModel
{
	#region ElementsCount
	private ElementsCount _elementsCount;

	/// <inheritdoc />
	ElementsCount IMemoryModel.ElementsCount
	{
		get
		{
			ThrowIfNoInit();
			return _elementsCount;
		}
	}

	#endregion

	#region IsInitialized

	/// <inheritdoc />
	bool IMemoryModel.IsInitialized => IsInitialized;

	#endregion

	#region Name
	private string _name;

	/// <inheritdoc />
	string IModel.Name
	{
		get
		{
			ThrowIfNoInit();
			return _name;
		}
	}

	#endregion

	#region Type
	private Type _type;

	/// <inheritdoc />
	Type IMemoryModel.Type
	{
		get
		{
			ThrowIfNoInit();
			return _type;
		}
	}
	#endregion

	#region Path
	private ModelPath _path;

	/// <inheritdoc />
	ModelPath IModel.Path
	{
		get
		{
			ThrowIfNoInit();
			return _path;
		}
	}
	#endregion

	/// <inheritdoc />
	IEnumerable<IPropertyData> IMemoryModel.GetModifiedProperties()
	{
		ThrowIfNoInit();
		return _propertiesData.Values.Where(x => x.IsModified);
	}

	/// <inheritdoc />
	IEnumerable<IPropertyData> IMemoryModel.GetProperties()
	{
		ThrowIfNoInit();
		return _propertiesData.Values;
	}
}