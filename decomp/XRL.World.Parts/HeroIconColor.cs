using System;

namespace XRL.World.Parts;

[Serializable]
public class HeroIconColor : IIconColorPart
{
	public HeroIconColor()
	{
		TextForeground = "&M";
		TextForegroundPriority = 100;
		TileForeground = "&M";
		TileForegroundPriority = 100;
	}
}
