using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModNanochelated : IModification
{
	public static readonly int ICON_COLOR_FOREGROUND_PRIORITY = 60;

	public static readonly int ICON_COLOR_DETAIL_PRIORITY = 80;

	public ModNanochelated()
	{
	}

	public ModNanochelated(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<Metal>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RemovePart<Metal>();
		Armor part = Object.GetPart<Armor>();
		Shield part2 = Object.GetPart<Shield>();
		if (part == null && part2 == null)
		{
			MeleeWeapon part3 = Object.GetPart<MeleeWeapon>();
			if (part3 != null)
			{
				part3.PenBonus--;
			}
		}
		else
		{
			if (part != null)
			{
				if (part.AV > 0)
				{
					part.AV--;
				}
				int num = (ParentObject.HasPart<ModVisored>() ? 1 : 0);
				if (part.DV < num)
				{
					part.DV++;
				}
			}
			if (part2 != null)
			{
				if (part2.AV > 0)
				{
					part2.AV--;
				}
				if (part2.DV < 0)
				{
					part2.DV++;
				}
			}
		}
		Object.IsOrganic = true;
		IncreaseDifficultyAndComplexityIfComplex(2, 1);
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
			E.AddAdjective("{{K|nanochelated}}");
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
		return "Nanochelated: This item's metals have been replaced with carbon fiber. -1 AV or penetration, +1 DV if below zero";
	}

	public override bool Render(RenderEvent E)
	{
		E.ApplyColors("&K", "y", ICON_COLOR_FOREGROUND_PRIORITY, ICON_COLOR_DETAIL_PRIORITY);
		return base.Render(E);
	}
}
