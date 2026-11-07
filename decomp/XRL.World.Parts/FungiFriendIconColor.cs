using System;

namespace XRL.World.Parts;

[Serializable]
public class FungiFriendIconColor : IIconColorPart
{
	public FungiFriendIconColor()
	{
		TextForeground = "&m";
		TextForegroundPriority = 90;
		TileForeground = "&m";
		TileForegroundPriority = 90;
		TileDetail = "w";
		TileDetailPriority = 90;
	}
}
