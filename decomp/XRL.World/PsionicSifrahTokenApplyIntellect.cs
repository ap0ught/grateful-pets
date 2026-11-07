using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class PsionicSifrahTokenApplyIntellect : SifrahToken
{
	public PsionicSifrahTokenApplyIntellect()
	{
		Description = "apply intellect";
		Tile = "Items/ms_intelligence.bmp";
		RenderString = "§";
		ColorString = "&B";
		DetailColor = 'Y';
	}
}
