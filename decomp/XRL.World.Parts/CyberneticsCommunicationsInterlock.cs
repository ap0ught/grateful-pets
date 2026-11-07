using System;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsCommunicationsInterlock : IPart
{
	public const string SKILL = "Persuasion_RebukeRobot";

	public const int DEFAULT_BONUS = 5;

	public int Bonus = 5;

	public bool SkillApplied;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetRebukeLevelEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ImplantedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetRebukeLevelEvent E)
	{
		E.Level += GetAvailableComputePowerEvent.AdjustUp(E.Actor, Bonus);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ParentObject.Implantee != null)
		{
			GenotypeEntry genotypeEntry = ParentObject.Implantee.genotypeEntry;
			if (genotypeEntry == null || genotypeEntry.Skills?.Contains("Persuasion_RebukeRobot") != true)
			{
				E.Postfix.AppendRules("For mutants, the first communications interlock allows the use of Rebuke Robot at a level of ability comparable to that of a True Kin with no communications interlock.");
			}
		}
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (!E.Implantee.HasPart("Persuasion_RebukeRobot"))
		{
			E.Implantee.AddSkill("Persuasion_RebukeRobot");
			SkillApplied = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		if (SkillApplied)
		{
			E.Implantee?.RemoveSkill("Persuasion_RebukeRobot");
			SkillApplied = false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
