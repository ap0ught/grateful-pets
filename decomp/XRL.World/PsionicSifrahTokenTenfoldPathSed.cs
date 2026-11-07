using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathSed : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathSed()
	{
		Description = "draw on the grace of Sed";
		Tile = "Items/ms_sed.bmp";
		RenderString = ")";
		ColorString = "&B";
		DetailColor = 'Y';
	}
}
