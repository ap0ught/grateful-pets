using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenRailAgainstInjustice : SifrahToken
{
	public SocialSifrahTokenRailAgainstInjustice()
	{
		Description = "rail against injustice";
		Tile = "Items/sw_gianthands.bmp";
		RenderString = "í";
		ColorString = "&w";
		TileColor = "&w";
		DetailColor = 'r';
	}
}
