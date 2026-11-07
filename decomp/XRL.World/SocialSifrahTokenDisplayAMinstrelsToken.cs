using System;
using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class SocialSifrahTokenDisplayAMinstrelsToken : SifrahPrioritizableToken
{
	public SocialSifrahTokenDisplayAMinstrelsToken()
	{
		Description = "display a minstrel's token";
		Tile = "Items/sw_token.bmp";
		RenderString = "\t";
		ColorString = "&y";
		DetailColor = 'm';
	}

	public override int GetPriority()
	{
		if (!IsAvailable())
		{
			return 0;
		}
		return int.MaxValue;
	}

	public override int GetTiebreakerPriority()
	{
		return int.MaxValue;
	}

	public bool IsAvailable()
	{
		return The.Player.ContainsBlueprint("Minstrel's Token");
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable())
		{
			Popup.ShowFail("You do not have a minstrel's token.");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}
}
