using System;

namespace XRL.World.Parts;

[Serializable]
public class KindlethumbedIconColor : IIconColorPart
{
	public KindlethumbedIconColor()
	{
		TextForeground = "&r";
		TextForegroundPriority = 90;
		TileForeground = "&r";
		TileForegroundPriority = 90;
	}
}
