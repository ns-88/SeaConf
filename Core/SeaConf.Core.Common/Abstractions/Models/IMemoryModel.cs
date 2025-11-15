using System.Reflection;
using SeaConf.Common.Attributes;
using SeaConf.Core.Common.Models;

namespace SeaConf.Core.Common.Abstractions.Models;

/// <summary>
/// Initialized configuration data model in memory.
/// </summary>
public interface IMemoryInitializedModel
{
	/// <summary>
	/// Initialization.
	/// </summary>
	/// <param name="memoryModel">Data model in memory.</param>
	/// <param name="propertiesData">Properties data.</param>
	void Initialize(IMemoryModel memoryModel, IReadOnlyDictionary<string, IPropertyData> propertiesData);
}

/// <summary>
/// Configuration data model in memory.
/// </summary>
public interface IMemoryModel : IModel
{
	/// <summary>
	/// Type.
	/// </summary>
	Type Type { get; }

	/// <summary>
	/// Initialization sign.
	/// </summary>
	bool IsInitialized { get; }

	/// <summary>
	/// Number of elements.
	/// </summary>
	ElementsCount ElementsCount { get; }

	/// <summary>
	/// Getting all properties.
	/// </summary>
	/// <returns>All properties.</returns>
	IEnumerable<IPropertyData> GetProperties();

	/// <summary>
	/// Getting modified properties.
	/// </summary>
	/// <returns>Modified properties.</returns>
	IEnumerable<IPropertyData> GetModifiedProperties();

	public static string GetName(MemberInfo memberInfo)
	{
		var typeName = memberInfo.Name;

		if (typeName.Length == 1)
		{
			return typeName;
		}

		if (typeName.StartsWith('I'))
		{
			typeName = typeName.Substring(1, typeName.Length - 1);
		}

		return typeName;
	}

	public static string GetName(MemberInfo memberInfo, ModelAttribute attribute)
	{
		return string.IsNullOrWhiteSpace(attribute.Name)
			? GetName(memberInfo)
			: attribute.Name;
	}
}