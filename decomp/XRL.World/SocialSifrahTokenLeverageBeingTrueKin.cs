using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenLeverageBeingTrueKin : SifrahToken
{
	public SocialSifrahTokenLeverageBeingTrueKin()
	{
		Description = "leverage being True Kin";
		Tile = "Items/ms_happy_face.png";
		RenderString = "\u0002";
		ColorString = "&Y";
		DetailColor = 'B';
	}
}
