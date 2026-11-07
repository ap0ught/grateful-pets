using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenTellAnInspiringTale : SifrahToken
{
	public SocialSifrahTokenTellAnInspiringTale()
	{
		Description = "tell an inspiring tale";
		Tile = "Items/sw_mask3.bmp";
		RenderString = "Q";
		ColorString = "&b";
		TileColor = "&B";
		DetailColor = 'M';
	}
}
