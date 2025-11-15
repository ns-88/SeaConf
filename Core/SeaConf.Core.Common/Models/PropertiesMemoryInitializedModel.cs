using SeaConf.Common.Infrastructure;
using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Common.Models;

public partial class PropertiesModel : IMemoryInitializedModel
{
	/// <inheritdoc />
	void IMemoryInitializedModel.Initialize(IMemoryModel memoryModel, IReadOnlyDictionary<string, IPropertyData> propertiesData)
	{
		_propertiesData = Guard.ThrowIfNull(propertiesData);
		_name = memoryModel.Name;
		_path = memoryModel.Path;
		_type = memoryModel.Type;
		_elementsCount = memoryModel.ElementsCount;

		SetInit();
	}
}