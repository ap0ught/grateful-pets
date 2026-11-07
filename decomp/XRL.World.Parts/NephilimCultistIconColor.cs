using System;

namespace XRL.World.Parts;

[Serializable]
public class NephilimCultistIconColor : IIconColorPart
{
	public NephilimCultistIconColor()
		: this(null, null)
	{
	}

	public NephilimCultistIconColor(string Foreground)
		: this(Foreground, Foreground)
	{
	}

	public NephilimCultistIconColor(string TextForeground, string TileForeground)
	{
		base.TextForeground = TextForeground;
		TextForegroundPriority = 95;
		base.TileForeground = TileForeground;
		TileForegroundPriority = 95;
	}
}
