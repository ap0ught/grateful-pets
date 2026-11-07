using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenDebateRationally : SifrahToken
{
	public SocialSifrahTokenDebateRationally()
	{
		Description = "debate rationally";
		Tile = "Items/sw_book1.bmp";
		RenderString = "\u0014";
		ColorString = "&w";
		DetailColor = 'W';
	}
}
