using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenBoastOfAccomplishments : SifrahToken
{
	public SocialSifrahTokenBoastOfAccomplishments()
	{
		Description = "boast of my accomplishments";
		Tile = "Items/sw_armlocks.bmp";
		RenderString = "\u0013";
		ColorString = "&M";
		DetailColor = 'm';
	}
}
