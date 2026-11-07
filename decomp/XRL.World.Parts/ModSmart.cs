using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModSmart : IModification
{
	public ModSmart()
	{
	}

	public ModSmart(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<MissileWeapon>())
		{
			return false;
		}
		if (!Object.HasPart<ModScoped>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		Smartgun part = Object.GetPart<Smartgun>();
		int modificationLevel = GetModificationLevel(Tier);
		if (part == null)
		{
			part = new Smartgun();
			part.Level = modificationLevel;
			Object.AddPart(part);
		}
		else if (part.Level < modificationLevel)
		{
			part.Level = modificationLevel;
		}
		int num = Math.Max((Tier >= 5) ? (10 - Tier) : (Tier + 2), 2);
		BootSequence part2 = Object.GetPart<BootSequence>();
		if (part2 == null)
		{
			part2 = new BootSequence();
			part2.BootTime = num;
			part2.ReadoutInName = true;
			part2.ReadoutInDescription = true;
			Object.AddPart(part2);
		}
		else if (part2.BootTime < num)
		{
			part2.BootTime = num;
		}
		IncreaseDifficultyAndComplexity(5, 2);
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
			E.AddAdjective("{{c|smart}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static int GetModificationLevel(int Tier)
	{
		return (int)Math.Ceiling(((float)Tier + 1f) / 1.8f);
	}

	public static string GetDescription(int Tier)
	{
		return "Smart: When powered and started up and the wielder has a HUD or techscanner equipped, this weapon's tracking scope makes it more accurate and gives " + ((Tier > 0) ? GetModificationLevel(Tier).Signed() : "a bonus") + " to hit a target aimed at.";
	}

	public string GetPhysicalDescription()
	{
		return "";
	}
}
