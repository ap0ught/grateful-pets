using System;

namespace XRL.World.Parts;

[Serializable]
public class QudzuSymbioteIconColor : IIconColorPart
{
	public QudzuSymbioteIconColor()
	{
		TextForeground = "&r";
		TextForegroundPriority = 90;
		TileForeground = "&r";
		TileForegroundPriority = 90;
	}
}
