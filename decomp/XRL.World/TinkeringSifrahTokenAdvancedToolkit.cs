using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenAdvancedToolkit : SifrahPrioritizableToken
{
	public TinkeringSifrahTokenAdvancedToolkit()
	{
		Description = "apply an advanced toolkit";
		Tile = "Items/sw_toolbox_large.bmp";
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
			if (part != null && part.TrackAsToolbox && part.PoweredDisassembleBonus >= 15 && part.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsUnusable()
	{
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			Toolbox part = item.GetPart<Toolbox>();
			if (part != null && part.TrackAsToolbox && part.PoweredDisassembleBonus >= 15 && !part.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
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
			if (IsUnusable())
			{
				Popup.ShowFail("You do not have a usable advanced toolkit.");
			}
			else
			{
				Popup.ShowFail("You do not have an advanced toolkit.");
			}
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			Toolbox part = item.GetPart<Toolbox>();
			if (part != null && part.TrackAsToolbox && part.PoweredInspectBonus >= 5 && part.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				part.ConsumeChargeIfOperational();
				break;
			}
		}
		base.UseToken(Game, Slot, ContextObject);
	}
}
