using System;

namespace XRL.World.Parts;

[Serializable]
public class ModSharp : IModification
{
	public ModSharp()
	{
	}

	public ModSharp(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "EdgeEnhancement";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.TryGetPart<MeleeWeapon>(out var Part) && (Part.Skill == "LongBlades" || Part.Skill == "ShortBlades" || Part.Skill == "Axe"))
		{
			return true;
		}
		if (Object.TryGetPart<ThrownWeapon>(out var Part2) && Part2.Attributes != null && (Part2.Attributes.HasDelimitedSubstring(' ', "LongBlades") || Part2.Attributes.HasDelimitedSubstring(' ', "ShortBlades") || Part2.Attributes.HasDelimitedSubstring(' ', "Axe")))
		{
			return true;
		}
		return false;
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
		if (E.Understood() && !E.Object.HasProperName && !E.Object.HasPart<ModKeen>())
		{
			E.AddAdjective("sharp", -20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!E.Object.HasPart<ModKeen>())
		{
			E.Postfix.AppendRules(GetDescription(Tier));
		}
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
		return "Sharp: +1 to penetration rolls";
	}
}
