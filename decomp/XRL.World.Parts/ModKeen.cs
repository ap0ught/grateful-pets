using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModKeen : IModification
{
	public ModKeen()
	{
	}

	public ModKeen(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "EdgeHyperEnhancement";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<ModSharp>();
	}

	public override void ApplyModification(GameObject Object)
	{
		MeleeWeapon part = Object.GetPart<MeleeWeapon>();
		if (part != null && (part.Skill == "LongBlades" || part.Skill == "ShortBlades" || part.Skill == "Axe"))
		{
			part.PenBonus++;
		}
		if (Object.TryGetPart<ThrownWeapon>(out var Part) && Part.Attributes != null && (Part.Attributes.HasDelimitedSubstring(' ', "LongBlades") || Part.Attributes.HasDelimitedSubstring(' ', "ShortBlades") || Part.Attributes.HasDelimitedSubstring(' ', "Axe")))
		{
			Part.PenetrationBonus++;
		}
		IncreaseDifficultyIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("keen", -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Keen: +2 to penetration rolls";
	}
}
