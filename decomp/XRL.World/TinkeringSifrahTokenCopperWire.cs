using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class TinkeringSifrahTokenCopperWire : SifrahPrioritizableToken
{
	public TinkeringSifrahTokenCopperWire()
	{
		Description = "use a length of copper wire";
		Tile = "Items/sw_copper_wire.bmp";
		RenderString = "í";
		ColorString = "&r";
		DetailColor = 'w';
	}

	public override int GetPriority()
	{
		return GetNumberAvailable();
	}

	public override int GetTiebreakerPriority()
	{
		return 0;
	}

	public int GetNumberAvailable(int Chosen = 0)
	{
		int num = -Chosen;
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.HasPart<Wire>())
			{
				num += item.Count;
			}
		}
		return num;
	}

	public bool IsAvailable(int Chosen = 0)
	{
		int num = 0;
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			if (item.HasPart<Wire>())
			{
				num += item.Count;
				if (num > Chosen)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string GetDescription(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		return Description + " [have {{C|" + GetNumberAvailable(Game.GetTimesChosen(this, Slot)) + "}}]";
	}

	public override bool GetDisabled(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (!IsAvailable(Game.GetTimesChosen(this, Slot)))
		{
			return true;
		}
		return base.GetDisabled(Game, Slot, ContextObject);
	}

	public override bool CheckTokenUse(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		int timesChosen = Game.GetTimesChosen(this, Slot);
		if (!IsAvailable(timesChosen))
		{
			if (timesChosen > 0)
			{
				Popup.ShowFail("You do not have any more copper wire.");
			}
			else
			{
				Popup.ShowFail("You do not have any copper wire.");
			}
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		GameObject gameObject = null;
		int num = int.MaxValue;
		foreach (GameObject item in The.Player.GetInventoryAndEquipment())
		{
			Wire part = item.GetPart<Wire>();
			if (part != null && part.Length < num)
			{
				gameObject = item;
			}
		}
		gameObject?.Destroy();
		base.UseToken(Game, Slot, ContextObject);
	}
}
