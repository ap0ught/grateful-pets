using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathKet : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathKet()
	{
		Description = "draw on the authority of Ket";
		Tile = "Items/ms_ket.bmp";
		RenderString = "^";
		ColorString = "&Y";
		DetailColor = 'Y';
	}
}
