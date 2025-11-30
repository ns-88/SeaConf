using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using SeaConf.Common;

namespace SeaConf.Core.Sources.XmlSource;

internal static class Helper
{
	public const string ValueAttributeName = "value";

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryGetElement(XContainer parentElement, string name, [MaybeNullWhen(false)] out XElement childElement)
	{
		childElement = parentElement.Elements().FirstOrDefault(x => x.Name.LocalName.Equals(name, StringComparison.Ordinal));
		return childElement != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static XElement CreateOrGetPropertyElement(XContainer collectionElement, string propertyElementName, string valueAttributeName, bool throwIfNull = true)
	{
		var propertyElement = collectionElement.Element(propertyElementName);

		if (throwIfNull && propertyElement == null)
		{
			throw new InvalidOperationException(string.Format(Resources.XmlDocumentElementNotExist, propertyElement));
		}

		if (propertyElement == null)
		{
			propertyElement = new XElement(propertyElementName);
			collectionElement.Add(propertyElement);
		}

		var valueAttribute = propertyElement.FirstAttribute;

		if (valueAttribute == null)
		{
			if (throwIfNull)
			{
				throw new InvalidOperationException(string.Format(Resources.XmlValueAttributeNotFound, propertyElementName));
			}

			valueAttribute = new XAttribute(valueAttributeName, string.Empty);

			propertyElement.Add(valueAttribute);
		}
		else
		{
			if (!valueAttribute.Name.LocalName.Equals(valueAttributeName, StringComparison.Ordinal))
			{
				throw new InvalidOperationException(string.Format(Resources.XmlValueAttributeNotFound, propertyElementName));
			}
		}

		return propertyElement;
	}
}