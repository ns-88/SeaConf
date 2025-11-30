using SeaConf.Core.Common.Abstractions.Models;

namespace SeaConf.Core.Common.Properties;

/// <inheritdoc />
public class Property : IProperty
{
	/// <inheritdoc />
	public string Name { get; }

	public Property(string name)
	{
		Name = name;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"Name = {Name}";
	}
}