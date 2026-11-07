using System.Xml;

namespace XRL;

internal class XmlUnsupportedElementException : XmlException
{
	public XmlUnsupportedElementException(XmlTextReader Reader)
		: base("unsupported element " + Reader.Name, Reader)
	{
	}
}
