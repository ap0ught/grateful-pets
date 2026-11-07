using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenFlatterInsincerely : SifrahToken
{
	public SocialSifrahTokenFlatterInsincerely()
	{
		Description = "flatter insincerely";
		Tile = "Items/sw_twohearted.bmp";
		RenderString = "\u0006";
		ColorString = "&K";
		DetailColor = 'W';
	}
}
