using System.Xml;

namespace XRL;

public class ZTBranchNode : ZoneTemplateNode
{
	public string Template;

	public override void Load(XmlReader Reader)
	{
		Template = Reader.GetAttribute("Template");
	}

	public override bool Execute(ZoneTemplateGenerationContext Context)
	{
		ZoneTemplateManager.Templates[Template].Execute(Context.Z);
		return base.Execute(Context);
	}
}
