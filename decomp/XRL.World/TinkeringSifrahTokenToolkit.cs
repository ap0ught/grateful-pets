using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenToolkit : SifrahPrioritizableToken
{
	public TinkeringSifrahTokenToolkit()
	{
		Description = "apply a toolkit";
		Tile = "Items/sw_toolbox.bmp";
		RenderString = "\b";
		ColorString = "&c";
		DetailColor = 'C';
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
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			Toolbox part = item.GetPart<Toolbox>();
			if (part != null && part.TrackAsToolbox)
			{
				return true;
			}
		}
		return false;
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
			Popup.ShowFail("You do not have a toolkit.");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			Toolbox part = item.GetPart<Toolbox>();
			if (part != null && part.TrackAsToolbox)
			{
				part.ConsumeChargeIfOperational();
				break;
			}
		}
		base.UseToken(Game, Slot, ContextObject);
	}
}
