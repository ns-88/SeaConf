using SeaConf.Common.Infrastructure;

namespace SeaConf.Core.Common.Models;

/// <summary>
/// Model data - type and name.
/// </summary>
public readonly struct ModelData : IEquatable<ModelData>
{
	/// <summary>
	/// Type.
	/// </summary>
	public readonly Type Type;

	/// <summary>
	/// Name.
	/// </summary>
	public readonly string Name;

	public ModelData(string name, Type type)
	{
		Name = Guard.ThrowIfEmptyString(name);
		Type = Guard.ThrowIfNull(type);
	}

	/// <inheritdoc />
	public bool Equals(ModelData other)
	{
		return Name.Equals(other.Name, StringComparison.Ordinal) && Type == other.Type;
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		return obj is ModelData other && Equals(other);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Type);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return Name;
	}
}