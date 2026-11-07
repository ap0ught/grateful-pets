using System;

namespace XRL.World.Parts;

[Serializable]
public class FungusRiddenIconColor : IIconColorPart
{
	public FungusRiddenIconColor()
	{
		TextForeground = "&m";
		TextForegroundPriority = 90;
		TileForeground = "&m";
		TileForegroundPriority = 90;
		TileDetail = "W";
		TileDetailPriority = 90;
	}
}
