using System;

namespace XRL.World.Parts;

[Serializable]
public class WardenIconColor : IIconColorPart
{
	public WardenIconColor()
	{
		TextForeground = "&B";
		TextForegroundPriority = 100;
		TileForeground = "&B";
		TileForegroundPriority = 100;
	}
}
