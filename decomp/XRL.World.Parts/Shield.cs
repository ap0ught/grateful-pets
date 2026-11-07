using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class Shield : IPart
{
	public string WornOn = "Arm";

	public int AV;

	public int DV;

	public int SpeedPenalty;

	[FieldSaveVersion(399)]
	public bool DisplayStats = true;

	[NonSerialized]
	public int Blocks;

	public override bool SameAs(IPart p)
	{
		Shield shield = p as Shield;
		if (shield.WornOn != WornOn)
		{
			return false;
		}
		if (shield.AV != AV)
		{
			return false;
		}
		if (shield.DV != DV)
		{
			return false;
		}
		if (shield.SpeedPenalty != SpeedPenalty)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetShieldBlockPreferenceEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != QueryEquippableListEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShieldBlockPreferenceEvent E)
	{
		E.Preference += AV * 100;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.SlotType == WornOn && E.Item == ParentObject && !E.List.Contains(E.Item) && (!E.RequireDesirable || E.Actor.HasSkill("Shield_Block")))
		{
			if (!E.RequirePossible)
			{
				E.List.Add(E.Item);
			}
			else
			{
				string usesSlots = E.Item.UsesSlots;
				if (!usesSlots.IsNullOrEmpty() && (E.SlotType != "Thrown Weapon" || usesSlots.Contains("Thrown Weapon")) && (E.SlotType != "Hand" || usesSlots.Contains("Hand")))
				{
					if (E.Actor.IsGiganticCreature)
					{
						if (E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
						{
							E.List.Add(E.Item);
						}
					}
					else if (E.SlotType == "Hand" || E.SlotType == "Missile Weapon" || !E.Item.IsGiganticEquipment || !E.Item.IsNatural())
					{
						E.List.Add(E.Item);
					}
				}
				else if (!E.Actor.IsGiganticCreature || E.Item.IsGiganticEquipment || E.Item.HasPropertyOrTag("GiganticEquippable") || E.Item.IsNatural())
				{
					int slotsRequiredFor = E.Item.GetSlotsRequiredFor(E.Actor, E.SlotType, FloorAtOne: false);
					if (slotsRequiredFor > 0 && slotsRequiredFor <= E.Actor.GetBodyPartCount(E.SlotType))
					{
						E.List.Add(E.Item);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		StatShifter.SetStatShift(E.Actor, "DV", DV);
		StatShifter.SetStatShift(E.Actor, "Speed", -SpeedPenalty);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		StatShifter.RemoveStatShifts(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (DisplayStats && E.Understood())
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("{{b|").Append('\u0004').Append("}}")
				.Append(AV)
				.Append(" {{K|")
				.Append('\t')
				.Append("}}")
				.Append(DV);
			E.AddTag(stringBuilder.ToString(), -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Shields only grant their AV when you successfully block an attack.");
		return base.HandleEvent(E);
	}
}
