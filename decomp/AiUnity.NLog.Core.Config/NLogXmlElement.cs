using System;
using System.Collections.Generic;
using System.Xml;

namespace AiUnity.NLog.Core.Config;

internal class NLogXmlElement
{
	public string LocalName { get; private set; }

	public Dictionary<string, string> AttributeValues { get; private set; }

	public IList<NLogXmlElement> Children { get; private set; }

	public string Value { get; private set; }

	public NLogXmlElement(string inputUri)
		: this()
	{
		using XmlReader xmlReader = XmlReader.Create(inputUri);
		xmlReader.MoveToContent();
		Parse(xmlReader);
	}

	public NLogXmlElement(XmlReader reader)
		: this()
	{
		Parse(reader);
	}

	private NLogXmlElement()
	{
		AttributeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Children = new List<NLogXmlElement>();
	}

	public IEnumerable<NLogXmlElement> Elements(string elementName)
	{
		List<NLogXmlElement> list = new List<NLogXmlElement>();
		foreach (NLogXmlElement child in Children)
		{
			if (child.LocalName.Equals(elementName, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(child);
			}
		}
		return list;
	}

	public string GetRequiredAttribute(string attributeName)
	{
		string optionalAttribute = GetOptionalAttribute(attributeName, null);
		if (optionalAttribute == null)
		{
			throw new NLogConfigurationException("Expected " + attributeName + " on <" + LocalName + " />");
		}
		return optionalAttribute;
	}

	public bool GetOptionalBooleanAttribute(string attributeName, bool defaultValue)
	{
		if (!AttributeValues.TryGetValue(attributeName, out var value))
		{
			return defaultValue;
		}
		return Convert.ToBoolean(value);
	}

	public string GetOptionalAttribute(string attributeName, string defaultValue)
	{
		if (!AttributeValues.TryGetValue(attributeName, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public void AssertName(params string[] allowedNames)
	{
		foreach (string value in allowedNames)
		{
			if (LocalName.Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
		}
		throw new InvalidOperationException("Assertion failed. Expected element name '" + string.Join("|", allowedNames) + "', actual: '" + LocalName + "'.");
	}

	private void Parse(XmlReader reader)
	{
		if (reader.MoveToFirstAttribute())
		{
			do
			{
				AttributeValues.Add(reader.LocalName, reader.Value);
			}
			while (reader.MoveToNextAttribute());
			reader.MoveToElement();
		}
		LocalName = reader.LocalName;
		if (reader.IsEmptyElement)
		{
			return;
		}
		while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType == XmlNodeType.CDATA || reader.NodeType == XmlNodeType.Text)
			{
				Value += reader.Value;
			}
			else if (reader.NodeType == XmlNodeType.Element)
			{
				Children.Add(new NLogXmlElement(reader));
			}
		}
	}
}
