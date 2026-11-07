using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Shield_ShieldWall : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandShieldWall";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 1 && !E.Actor.IsFrozen() && E.Actor.GetShield() != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			GameObject shield = ParentObject.GetShield();
			if (shield == null)
			{
				return ParentObject.Fail("You have no shield to raise.");
			}
			if (!ParentObject.CanMoveExtremities("ShieldWall", ShowMessage: true))
			{
				return false;
			}
			ParentObject.ApplyEffect(new ShieldWall(3, shield));
			CooldownMyActivatedAbility(ActivatedAbilityID, 30);
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Shield Wall", COMMAND_NAME, "Skills", null, "\u0004");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 30);
	}
}
