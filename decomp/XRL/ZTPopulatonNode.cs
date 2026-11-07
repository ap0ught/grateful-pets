using System.Xml;
using XRL.World.ZoneBuilders;

namespace XRL;

public class ZTPopulatonNode : ZoneTemplateNode
{
	public string Table;

	public override void Load(XmlReader Reader)
	{
		Table = Reader.GetAttribute("Table");
	}

	public override bool Execute(ZoneTemplateGenerationContext Context)
	{
		ZoneBuilderSandbox.PlacePopulationInRegion(Context.Z, Context.Regions.Regions[Context.CurrentRegion], VariableReplace(Table, Context), Hint);
		return base.Execute(Context);
	}
}
