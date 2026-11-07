using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenApplyAncientLore : SifrahToken
{
	public PsionicSifrahTokenApplyAncientLore()
	{
		Description = "apply ancient lore";
		Tile = "Items/sw_scroll1.bmp";
		RenderString = "ë";
		ColorString = "&w";
		DetailColor = 'W';
	}
}
