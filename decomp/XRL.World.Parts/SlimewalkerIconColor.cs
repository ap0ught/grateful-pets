using System;

namespace XRL.World.Parts;

[Serializable]
public class SlimewalkerIconColor : IIconColorPart
{
	public SlimewalkerIconColor()
	{
		TextForeground = "&g";
		TextForegroundPriority = 90;
		TileForeground = "&g";
		TileForegroundPriority = 90;
	}
}
