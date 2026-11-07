using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBladesDeathblow : LongBladesSkillBase
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public static readonly int COOLDOWN = 100;

	public static readonly int DURATION = 10;

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("En Garde!", "CommandDeathblow", "Skills", null, "\u009f");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Duration", DURATION);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanRefreshAbilityEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanRefreshAbilityEvent E)
	{
		if (E.Ability.ID == ActivatedAbilityID && ParentObject.HasEffect(typeof(LongbladeEffect_EnGarde)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}
}
