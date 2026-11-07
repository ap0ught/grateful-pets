using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenDiscipline : SifrahToken
{
	public PsionicSifrahTokenDiscipline()
	{
		Description = "draw on reserves of self-discipline";
		Tile = "Items/sw_gem.bmp";
		RenderString = "è";
		ColorString = "&K";
		DetailColor = 'Y';
	}
}
