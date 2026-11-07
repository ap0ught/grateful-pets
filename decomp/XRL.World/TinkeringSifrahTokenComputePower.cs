using System;
using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenComputePower : SifrahPrioritizableToken
{
	public int Amount = 1;

	public TinkeringSifrahTokenComputePower()
	{
		Description = "apply compute power";
		Tile = "Items/sw_skillsoft.bmp";
		RenderString = "ä";
		ColorString = "&C";
		DetailColor = 'B';
	}

	public TinkeringSifrahTokenComputePower(int Amount)
		: this()
	{
		if (Amount < 1)
		{
			Amount = 1;
		}
		this.Amount = Amount;
		Description = "apply {{C|" + Amount + "}} " + ((Amount == 1) ? "unit" : "units") + " of compute power";
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
		if (!IsAvailable())
		{
			return -1;
		}
		return int.MaxValue;
	}

	public bool IsAvailable()
	{
		return GetAvailableComputePowerEvent.GetFor(The.Player) >= Amount;
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
			Popup.ShowFail("You do not have " + Amount + " " + ((Amount == 1) ? "unit" : "units") + " of compute power available on the local lattice.");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}
}
