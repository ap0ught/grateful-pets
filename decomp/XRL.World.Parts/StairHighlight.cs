using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class StairHighlight : IPart
{
	public bool bEnabled = true;

	public override bool FinalRender(RenderEvent E)
	{
		if (bEnabled && Options.HighlightStairs && ParentObject.CurrentCell != null && ParentObject.CurrentCell.IsExplored() && !ParentObject.CurrentCell.HasObjectWithPropertyOrTag("SuspendedPlatform"))
		{
			E.CustomDraw = true;
			E.ColorString = (Options.UseTiles ? "&y^M" : "&Y^M");
			E.DetailColor = "Y";
		}
		return true;
	}
}
