using System;

namespace XRL.World.Parts;

[Serializable]
public class FirethumbedIconColor : IIconColorPart
{
	public FirethumbedIconColor()
	{
		TextForeground = "&R";
		TextForegroundPriority = 90;
		TileForeground = "&R";
		TileForegroundPriority = 90;
	}
}
