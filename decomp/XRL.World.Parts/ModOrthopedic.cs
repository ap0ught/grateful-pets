using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModOrthopedic : IModification
{
	public const string SAVE_VS = "Move,Knockdown,Restraint";

	public ModOrthopedic()
	{
	}

	public ModOrthopedic(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "OrthopedicSystems";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Feet", "Back");
	}

	public override void ApplyModification(GameObject Object)
	{
		int num = -Tier;
		MoveCostMultiplier part = ParentObject.GetPart<MoveCostMultiplier>();
		if (part != null)
		{
			part.Amount += num;
		}
		else
		{
			ParentObject.AddPart(new MoveCostMultiplier(num));
		}
		SaveModifier part2 = Object.GetPart<SaveModifier>();
		if (part2 != null)
		{
			part2.Amount += GetSaveModifierAmount(Tier);
		}
		else
		{
			part2 = Object.AddPart<SaveModifier>();
			part2.Vs = "Move,Knockdown,Restraint";
			part2.Amount = GetSaveModifierAmount(Tier);
			part2.ShowInShortDescription = false;
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
			E.AddAdjective("orthopedic");
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
		return "Orthopedic: This item grants bonus move speed and " + (SavingThrows.GetSaveBonusDescription(GetSaveModifierAmount(Tier), "Move,Knockdown,Restraint") ?? "no effect");
	}

	public static int GetSaveModifierAmount(int Tier)
	{
		return 1 + Tier / 6;
	}
}
