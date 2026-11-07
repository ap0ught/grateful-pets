using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathHok : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathHok()
	{
		Description = "draw on the creativity of Hok";
		Tile = "Items/ms_hok.bmp";
		RenderString = ")";
		ColorString = "&y";
		DetailColor = 'Y';
	}
}
