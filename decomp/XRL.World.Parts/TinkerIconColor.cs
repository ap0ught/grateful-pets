using System;

namespace XRL.World.Parts;

[Serializable]
public class TinkerIconColor : IIconColorPart
{
	public TinkerIconColor()
	{
		TextForeground = "&c";
		TextForegroundPriority = 100;
		TileForeground = "&c";
		TileForegroundPriority = 100;
	}
}
