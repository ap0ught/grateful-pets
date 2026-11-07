using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class WaterRitualBuyItem : IWaterRitualPart
{
	public const int TYPE_FACTION = 1;

	public const int TYPE_VALUED = 2;

	public GameObject Item;

	public int Type;

	public string Source
	{
		set
		{
			string text = value.ToLowerInvariant();
			int type = ((text == "faction") ? 1 : ((text == "valued") ? 2 : 0));
			Type = type;
		}
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != PrepareTextEvent.ID)
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public bool SetFactionItem()
	{
		string text = (WaterRitual.Alternative ? WaterRitual.RecordFaction.WaterRitualAltItemBlueprint : WaterRitual.RecordFaction.WaterRitualItemBlueprint);
		if (text.IsNullOrEmpty() || WaterRitual.Record.numItems < 1)
		{
			return false;
		}
		Item = The.Speaker.HasItemWithBlueprint(text);
		if (Item == null && WaterRitual.Record.canGenerateItem)
		{
			The.Speaker.ReceiveObject(text);
			Item = The.Speaker.HasItemWithBlueprint(text);
		}
		Reputation = (WaterRitual.Alternative ? WaterRitual.RecordFaction.WaterRitualAltItemCost : WaterRitual.RecordFaction.WaterRitualItemCost);
		WaterRitual.Record.canGenerateItem = false;
		return Item != null;
	}

	public override void Awake()
	{
		if (Type == 1)
		{
			if (!SetFactionItem())
			{
				return;
			}
		}
		else if (Type == 2)
		{
			if (WaterRitual.Record.numGifts < 1 || !WaterRitual.RecordFaction.WaterRitualBuyMostValuableItem)
			{
				return;
			}
			Item = The.Speaker.GetMostValuableItem();
			if (!GameObject.Validate(ref Item))
			{
				return;
			}
		}
		if (Reputation <= 0)
		{
			Reputation = 5;
			if (Item.TryGetPart<Commerce>(out var Part))
			{
				Reputation = Math.Max(5, (int)(Part.Value / 4.0));
			}
		}
		Reputation = GetWaterRitualCostEvent.GetFor(The.Player, The.Speaker, "Item", Reputation);
		Visible = true;
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		E.Object = Item;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		if (UseReputation())
		{
			Item = Item.RemoveOne();
			Item.UnequipAndRemove();
			Popup.Show(The.Speaker.Does("gift", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " you " + Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "!");
			The.Player.Inventory.AddObject(Item);
			if (Type == 1)
			{
				WaterRitual.Record.numItems--;
			}
			else if (Type == 2)
			{
				WaterRitual.Record.numGifts--;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[{{" + Numeric + "|" + GetReputationCost() + "}} reputation]}}";
		return false;
	}
}
