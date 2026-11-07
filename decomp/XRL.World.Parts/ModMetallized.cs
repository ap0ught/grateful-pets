using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModMetallized : IModification
{
	public static readonly int ICON_COLOR_FOREGROUND_PRIORITY = 50;

	public static readonly int ICON_COLOR_DETAIL_PRIORITY = 90;

	public ModMetallized()
	{
	}

	public ModMetallized(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.Physics == null)
		{
			return false;
		}
		if (Object.HasPart<Metal>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.AddPart<Metal>();
		Armor part = Object.GetPart<Armor>();
		Shield part2 = Object.GetPart<Shield>();
		if (part == null && part2 == null)
		{
			MeleeWeapon part3 = Object.GetPart<MeleeWeapon>();
			if (part3 != null)
			{
				part3.PenBonus++;
			}
		}
		else
		{
			if (part != null)
			{
				part.AV++;
			}
			if (part2 != null)
			{
				part2.AV++;
			}
		}
		Object.IsOrganic = false;
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
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
			E.AddAdjective("{{c|metallized}}", 20);
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
		return "Metallized: +1 AV or penetration";
	}

	public override bool Render(RenderEvent E)
	{
		E.ApplyColors("&c", "C", ICON_COLOR_FOREGROUND_PRIORITY, ICON_COLOR_DETAIL_PRIORITY);
		return base.Render(E);
	}
}
