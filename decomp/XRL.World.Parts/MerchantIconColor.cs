using System;

namespace XRL.World.Parts;

[Serializable]
public class MerchantIconColor : IIconColorPart
{
	public MerchantIconColor()
	{
		TextForeground = "&W";
		TextForegroundPriority = 100;
		TileForeground = "&W";
		TileForegroundPriority = 100;
	}
}
