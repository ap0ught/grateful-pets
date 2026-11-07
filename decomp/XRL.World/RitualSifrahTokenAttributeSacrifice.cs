using System;
using XRL.UI;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RitualSifrahTokenAttributeSacrifice : SifrahPrioritizableToken
{
	public string Attribute;

	public RitualSifrahTokenAttributeSacrifice()
	{
		Description = "sacrifice a point of an attribute";
		Tile = "Creatures/sw_pulsed_field_magnet.bmp";
		RenderString = "ã";
		ColorString = "&K";
		DetailColor = 'r';
	}

	public RitualSifrahTokenAttributeSacrifice(string Attribute)
		: this()
	{
		this.Attribute = Attribute;
		switch (this.Attribute)
		{
		case "Strength":
			Tile = "Items/ms_strength_sacrifice.bmp";
			ColorString = "&G";
			break;
		case "Agility":
			Tile = "Items/ms_agility_sacrifice.bmp";
			ColorString = "&W";
			break;
		case "Toughness":
			Tile = "Items/ms_toughness_sacrifice.bmp";
			ColorString = "&w";
			break;
		case "Intelligence":
			Tile = "Items/ms_intelligence_sacrifice.bmp";
			ColorString = "&B";
			break;
		case "Willpower":
			Tile = "Items/ms_willpower_sacrifice.bmp";
			ColorString = "&Y";
			break;
		case "Ego":
			Tile = "Items/ms_ego_sacrifice.bmp";
			ColorString = "&M";
			break;
		}
		Description = "sacrifice a point of " + Attribute;
	}

	public override int GetPriority()
	{
		if (!IsAvailable())
		{
			return 0;
		}
		return 1 + The.Player.Stat("Attribute") / 10;
	}

	public override int GetTiebreakerPriority()
	{
		return 0;
	}

	public bool IsAvailable()
	{
		return The.Player.Stat(Attribute) > 1;
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
			Popup.ShowFail("Your " + Attribute + " is too depleted to do that.");
			return false;
		}
		return base.CheckTokenUse(Game, Slot, ContextObject);
	}

	public override int GetPowerup(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		if (Slot.CurrentMove == Slot.Token)
		{
			return 3;
		}
		return base.GetPowerup(Game, Slot, ContextObject);
	}

	public override void UseToken(SifrahGame Game, SifrahSlot Slot, GameObject ContextObject)
	{
		The.Player.GetStat(Attribute).BaseValue--;
		base.UseToken(Game, Slot, ContextObject);
	}
}
