using System;

namespace XRL.World.Parts;

[Serializable]
public class ModGesticulating : IModification
{
	public ModGesticulating()
	{
	}

	public ModGesticulating(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Floating Nearby", null, null, null, AllowGeneric: false, AllowMagnetized: true, AllowPartial: false, Invert: true);
	}

	public override void ApplyModification(GameObject Object)
	{
		Armor part = Object.GetPart<Armor>();
		if (Object.UsesSlots == null)
		{
			string text = ((part == null || !(part.WornOn != "*")) ? "Hand" : part.WornOn);
			Object.UsesSlots = text + ",Floating Nearby";
		}
		else
		{
			Object.UsesSlots += ",Floating Nearby";
		}
		if (part != null)
		{
			part.Strength += GetStrengthBonus(Tier);
		}
		else
		{
			string text2 = GetStrengthBonus(Tier).ToString();
			EquipStatBoost.AppendBoostOnEquip(Object, "Strength:" + text2);
		}
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{m|gesticulating}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static int GetStrengthBonus(int Tier)
	{
		if (Tier <= 3)
		{
			return 2;
		}
		if (Tier <= 4)
		{
			return 3;
		}
		if (Tier <= 5)
		{
			return 4;
		}
		if (Tier <= 6)
		{
			return 5;
		}
		_ = 7;
		return 6;
	}

	public static string GetDescription(int Tier)
	{
		return "Gesticulating: This item grants +" + GetStrengthBonus(Tier) + " Strength but disallows the use of the Floating Nearby equipment slot.";
	}

	public string GetInstanceDescription()
	{
		return "Gesticulating: This item grants +" + GetStrengthBonus(Tier) + " Strength but disallows the use of the Floating Nearby equipment slot.";
	}
}
