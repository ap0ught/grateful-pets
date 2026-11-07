using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathVur : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathVur()
	{
		Description = "draw on the might of Vur";
		Tile = "Items/ms_vur.bmp";
		RenderString = "(";
		ColorString = "&R";
		DetailColor = 'Y';
	}
}
