using System.Xml.Linq;
using SeaConf.Common.Infrastructure;
using SeaConf.Core.Common.Abstractions;
using SeaConf.Core.Common.Abstractions.Models;
using SeaConf.Core.Common.Models;
using SeaConf.Core.Common.Properties;

namespace SeaConf.Core.Sources.XmlSource;

/// <summary>
/// Configuration data model in xml-file.
/// </summary>
internal sealed class XmlStorageModel : StorageModelBase
{
	private readonly XElement _element;

	private XmlStorageModel(string name, ModelPath path, XElement element) : base(name, path)
	{
		_element = element;
	}

	private static string GetName(XElement element)
	{
		return element.Name.LocalName;
	}

	public static XmlStorageModel FromElement(XElement element)
	{
		var name = GetName(element);
		return new XmlStorageModel(name, new ModelPath(name), element);
	}

	public static XmlStorageModel FromElement(XElement element, ModelPath path)
	{
		var name = GetName(element);
		return new XmlStorageModel(name, path, element);
	}

	/// <inheritdoc />
	public override ValueTask<IReadOnlyList<INode>> GetDescendantNodesAsync()
	{
		var nodes = new List<INode>();

		foreach (var element in _element.Elements())
		{
			if (element.HasAttributes)
			{
				continue;
			}

			var name = GetName(element);

			nodes.Add(new XmlStorageModel(name, new ModelPath(name, Path), element));
		}

		return new ValueTask<IReadOnlyList<INode>>(nodes);
	}

	/// <inheritdoc />
	public override ValueTask AddPropertyAsync(IProperty property)
	{
		Guard.ThrowIfNull(property);

		Helper.CreateOrGetPropertyElement(_element, property.Name, Helper.ValueAttributeName, false);

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public override ValueTask DeletePropertyAsync(IProperty property)
	{
		Guard.ThrowIfNull(property);

		var element = Helper.CreateOrGetPropertyElement(_element, property.Name, Helper.ValueAttributeName);

		element.Remove();

		return ValueTask.CompletedTask;
	}

	/// <inheritdoc />
	public override IEnumerable<IProperty> GetProperties()
	{
		foreach (var element in _element.Elements())
		{
			var attributeName = element.FirstAttribute?.Name.LocalName;

			if (attributeName?.Equals(Helper.ValueAttributeName, StringComparison.Ordinal) == true)
			{
				yield return new Property(element.Name.LocalName);
			}
		}
	}

	/// <inheritdoc />
	public override IWriter CreateWriter()
	{
		return new XmlReaderWriter(_element);
	}

	/// <inheritdoc />
	public override IReader CreateReader()
	{
		return new XmlReaderWriter(_element);
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"Name = {Name}, Element = {_element.Name}";
	}
}