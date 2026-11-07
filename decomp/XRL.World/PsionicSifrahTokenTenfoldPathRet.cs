using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenTenfoldPathRet : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathRet()
	{
		Description = "draw on the beauty of Ret";
		Tile = "Items/ms_ret.bmp";
		RenderString = "*";
		ColorString = "&w";
		DetailColor = 'Y';
	}
}
