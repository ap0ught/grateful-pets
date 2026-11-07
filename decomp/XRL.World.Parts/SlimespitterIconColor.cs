using System;

namespace XRL.World.Parts;

[Serializable]
public class SlimespitterIconColor : IIconColorPart
{
	public SlimespitterIconColor()
	{
		TextForeground = "&G";
		TextForegroundPriority = 90;
		TileForeground = "&G";
		TileForegroundPriority = 90;
	}
}
