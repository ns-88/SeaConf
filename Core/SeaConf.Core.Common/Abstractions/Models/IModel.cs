using SeaConf.Core.Common.Models;

namespace SeaConf.Core.Common.Abstractions.Models;

/// <summary>
/// Configuration data model.
/// </summary>
public interface IModel
{
	/// <summary>
	/// Name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Path.
	/// </summary>
	ModelPath Path { get; }
}