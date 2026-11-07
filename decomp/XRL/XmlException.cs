using System;
using System.Xml;

namespace XRL;

internal class XmlException : Exception
{
	public XmlException(string Message, XmlTextReader Reader)
		: base(Message + " at line " + Reader.LineNumber)
	{
	}
}
