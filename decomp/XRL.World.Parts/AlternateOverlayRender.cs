using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class AlternateOverlayRender : IPart
{
	public string ColorString;

	public string DetailColor;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool OverlayRender(RenderEvent E)
	{
		if (!string.IsNullOrEmpty(ColorString) && Globals.RenderMode == RenderModeType.Tiles)
		{
			E.ColorString = ColorString;
		}
		if (!string.IsNullOrEmpty(DetailColor) && Globals.RenderMode == RenderModeType.Tiles)
		{
			E.DetailColor = DetailColor;
		}
		return true;
	}
}
