using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenCrackAJoke : SifrahToken
{
	public SocialSifrahTokenCrackAJoke()
	{
		Description = "crack a joke";
		Tile = "Items/sw_mask.bmp";
		RenderString = "\u0001";
		ColorString = "&C";
		DetailColor = 'W';
	}
}
