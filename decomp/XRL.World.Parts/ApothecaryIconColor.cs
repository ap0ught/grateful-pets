using System;

namespace XRL.World.Parts;

[Serializable]
public class ApothecaryIconColor : IIconColorPart
{
	public ApothecaryIconColor()
	{
		TextForeground = "&g";
		TextForegroundPriority = 100;
		TileForeground = "&g";
		TileForegroundPriority = 100;
	}
}
