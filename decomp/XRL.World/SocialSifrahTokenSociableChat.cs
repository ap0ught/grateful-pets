using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenSociableChat : SifrahToken
{
	public SocialSifrahTokenSociableChat()
	{
		Description = "sociable chat";
		Tile = "Items/sw_cherubic.bmp";
		RenderString = "\u0002";
		ColorString = "&w";
		DetailColor = 'W';
	}
}
