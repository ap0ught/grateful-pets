using System.Xml;

namespace XRL;

public class ZTBuilderNode : ZoneTemplateNode
{
	public string Class;

	public override void Load(XmlReader Reader)
	{
		Class = Reader.GetAttribute("Class");
	}
}
