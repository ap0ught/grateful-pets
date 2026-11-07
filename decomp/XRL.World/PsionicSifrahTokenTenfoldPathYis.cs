using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathYis : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathYis()
	{
		Description = "draw on the depths of Yis";
		Tile = "Items/ms_yis.bmp";
		RenderString = "*";
		ColorString = "&m";
		DetailColor = 'Y';
	}
}
