using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathBin : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathBin()
	{
		Description = "draw on the insights of Bin";
		Tile = "Items/ms_bin.bmp";
		RenderString = "(";
		ColorString = "&K";
		DetailColor = 'Y';
	}
}
