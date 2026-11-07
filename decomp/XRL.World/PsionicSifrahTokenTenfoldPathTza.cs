using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathTza : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathTza()
	{
		Description = "draw on the constancy of Tza";
		Tile = "Items/ms_tza.bmp";
		RenderString = ")";
		ColorString = "&c";
		DetailColor = 'Y';
	}
}
