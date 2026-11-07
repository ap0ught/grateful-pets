using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ModCleated : IModification
{
	public const string SAVE_VS = "Move,Knockdown,Restraint";

	public ModCleated()
	{
	}

	public ModCleated(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "StabilityAssist";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!IModification.CheckWornSlot(Object, "Feet", "Back"))
		{
			return false;
		}
		SaveModifier part = Object.GetPart<SaveModifier>();
		if (part != null && part.Vs != "Move,Knockdown,Restraint")
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		SaveModifier part = Object.GetPart<SaveModifier>();
		if (part != null)
		{
			part.Amount += GetSaveModifierAmount(Tier);
		}
		else
		{
			part = Object.AddPart<SaveModifier>();
			part.Vs = "Move,Knockdown,Restraint";
			part.Amount = GetSaveModifierAmount(Tier);
			part.ShowInShortDescription = false;
		}
		IncreaseDifficultyIfComplex(1);
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
			E.AddWithClause("cleats");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Fitted with cleats: " + (SavingThrows.GetSaveBonusDescription(GetSaveModifierAmount(Tier), "Move,Knockdown,Restraint") ?? "no effect");
	}

	public static int GetSaveModifierAmount(int Tier)
	{
		return 2 + Tier / 4;
	}
}
