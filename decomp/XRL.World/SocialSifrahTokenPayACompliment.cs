using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenPayACompliment : SifrahToken
{
	public SocialSifrahTokenPayACompliment()
	{
		Description = "pay a compliment";
		Tile = "Items/ms_face_heart.png";
		RenderString = "\u0003";
		ColorString = "&M";
		DetailColor = 'W';
	}
}
