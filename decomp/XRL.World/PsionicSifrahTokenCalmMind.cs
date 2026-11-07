using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenCalmMind : SifrahToken
{
	public PsionicSifrahTokenCalmMind()
	{
		Description = "calm mind";
		Tile = "Items/ms_willpower.bmp";
		RenderString = "÷";
		ColorString = "&Y";
		DetailColor = 'Y';
	}
}
