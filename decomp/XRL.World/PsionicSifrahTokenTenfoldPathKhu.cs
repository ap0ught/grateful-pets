using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathKhu : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathKhu()
	{
		Description = "draw on the solidity of Khu";
		Tile = "Items/ms_khu.bmp";
		RenderString = "*";
		ColorString = "&g";
		DetailColor = 'Y';
	}
}
