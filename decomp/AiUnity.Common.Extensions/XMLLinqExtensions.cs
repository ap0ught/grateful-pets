using System;
using System.Xml.Linq;

namespace AiUnity.Common.Extensions;

public static class XMLLinqExtensions
{
	public static void MoveElementUp(this XElement element)
	{
		XNode previousNode = element.PreviousNode;
		while (previousNode != null && !(previousNode is XElement))
		{
			previousNode = previousNode.PreviousNode;
		}
		if (previousNode == null)
		{
			throw new ArgumentException("Nowhere to move element to!");
		}
		element.Remove();
		previousNode.AddBeforeSelf(element);
	}

	public static void MoveElementDown(this XElement element)
	{
		XNode nextNode = element.NextNode;
		while (nextNode != null && !(nextNode is XElement))
		{
			nextNode = nextNode.NextNode;
		}
		if (nextNode == null)
		{
			throw new ArgumentException("Nowhere to move element to!");
		}
		element.Remove();
		nextNode.AddAfterSelf(element);
	}

	public static XAttribute GetOrSetAttribute(this XElement element, string attributeName, string defaultValue)
	{
		XAttribute xAttribute = element.Attribute(attributeName);
		if (xAttribute == null)
		{
			xAttribute = new XAttribute(attributeName, defaultValue);
			element.Add(xAttribute);
		}
		return xAttribute;
	}
}
