using System;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenEmpathy : SifrahToken
{
	public SocialSifrahTokenEmpathy()
	{
		Description = "subtly employ empathy";
		Tile = "Items/sw_esper.bmp";
		RenderString = "÷";
		ColorString = "&M";
		DetailColor = 'W';
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!UsabilityCheckedThisTurn && !The.Player.CanMakeEmpathicContactWith(ContextObject))
		{
			DisabledThisTurn = true;
			return false;
		}
		return true;
	}
}
