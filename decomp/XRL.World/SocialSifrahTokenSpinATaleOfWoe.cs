using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenSpinATaleOfWoe : SifrahToken
{
	public SocialSifrahTokenSpinATaleOfWoe()
	{
		Description = "spin a tale of woe";
		Tile = "Items/sw_mask3.bmp";
		RenderString = "Q";
		ColorString = "&b";
		TileColor = "&B";
		DetailColor = 'b';
	}
}
