using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBladesDuelingStance : LongBladesSkillBase
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		if (ParentObject.HasPart<LongBladesImprovedDuelistStance>())
		{
			stats.Set("HitBonus", 3, changes: true, 1);
			stats.AddChangePostfix("To-hit bonus", 1, "Improved Dueling Stance");
		}
		else
		{
			stats.Set("HitBonus", 2);
		}
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Dueling Stance", "CommandDuelingStance", "Stances", "+2/3 to hit while wielding a long blade in your primary hand", "\u009f");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
