using System.Text.RegularExpressions;
using System.Xml;

namespace AiUnity.NLog.Core.Internal;

public static class XmlHelper
{
	private static readonly Regex InvalidXmlChars = new Regex("(?<![\\uD800-\\uDBFF])[\\uDC00-\\uDFFF]|[\\uD800-\\uDBFF](?![\\uDC00-\\uDFFF])|[\\x00-\\x08\\x0B\\x0C\\x0E-\\x1F\\x7F-\\x9F\\uFEFF\\uFFFE\\uFFFF]");

	private static string RemoveInvalidXmlChars(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			return InvalidXmlChars.Replace(text, "");
		}
		return "";
	}

	public static void WriteAttributeSafeString(this XmlWriter writer, string prefix, string localName, string ns, string value)
	{
		writer.WriteAttributeString(RemoveInvalidXmlChars(prefix), RemoveInvalidXmlChars(localName), RemoveInvalidXmlChars(ns), RemoveInvalidXmlChars(value));
	}

	public static void WriteAttributeSafeString(this XmlWriter writer, string thread, string localName)
	{
		writer.WriteAttributeString(RemoveInvalidXmlChars(thread), RemoveInvalidXmlChars(localName));
	}

	public static void WriteElementSafeString(this XmlWriter writer, string prefix, string localName, string ns, string value)
	{
		writer.WriteElementString(RemoveInvalidXmlChars(prefix), RemoveInvalidXmlChars(localName), RemoveInvalidXmlChars(ns), RemoveInvalidXmlChars(value));
	}

	public static void WriteSafeCData(this XmlWriter writer, string text)
	{
		writer.WriteCData(RemoveInvalidXmlChars(text));
	}
}
