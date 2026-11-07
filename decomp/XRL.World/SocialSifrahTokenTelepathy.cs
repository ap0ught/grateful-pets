using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenTelepathy : SifrahToken
{
	public SocialSifrahTokenTelepathy()
	{
		Description = "subtly employ telepathy";
		Tile = "Items/sw_esper.bmp";
		RenderString = "÷";
		ColorString = "&b";
		DetailColor = 'C';
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!UsabilityCheckedThisTurn && !The.Player.CanMakeTelepathicContactWith(ContextObject))
		{
			DisabledThisTurn = true;
			return false;
		}
		return true;
	}
}
