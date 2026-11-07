using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathHod : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathHod()
	{
		Description = "draw on the majesty of Hod";
		Tile = "Items/ms_hod.bmp";
		RenderString = "(";
		ColorString = "&O";
		DetailColor = 'Y';
	}
}
