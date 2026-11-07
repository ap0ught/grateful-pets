using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Multiweapon_Flurry : BaseSkill
{
	public const string ABL_CMD = "CommandFlurry";

	public static readonly int COOLDOWN = 60;

	public Guid ActivatedAbilityID = Guid.Empty;

	public void CollectStats(Templates.StatCollector stats)
	{
		int num = COOLDOWN;
		if (ParentObject.HasSkill("Multiweapon_Expertise"))
		{
			stats.AddChangePostfix("Cooldown", -10, "Multiweapon Expertise");
			num -= 10;
		}
		if (ParentObject.HasSkill("Multiweapon_Mastery"))
		{
			stats.AddChangePostfix("Cooldown", -10, "Multiweapon Mastery");
			num -= 10;
		}
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), num, num - COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<GetMeleeAttackChanceEvent>.ID)
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

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance == 1 && !ParentObject.HasEffect<Burrowed>() && ParentObject.CanMoveExtremities("Flurry") && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandFlurry");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (E.Properties.HasDelimitedSubstring(',', "Flurrying"))
		{
			E.SetFinalizedChance((E.Intrinsic && !E.Consecutive) ? 100 : 0);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandFlurry");
		base.Register(Object, Registrar);
	}

	public bool PerformFlurry()
	{
		if (!ParentObject.CheckFrozen())
		{
			return false;
		}
		if (OnWorldMap)
		{
			ParentObject.Fail("You cannot do that on the world map.");
			return false;
		}
		if (ParentObject.HasEffect<Burrowed>())
		{
			ParentObject.Fail("You cannot do that while burrowed.");
			return false;
		}
		if (!ParentObject.CanMoveExtremities("Flurry", ShowMessage: true))
		{
			return false;
		}
		Cell cell = PickDirection("Flurry");
		if (cell == null)
		{
			return false;
		}
		GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: false, IgnoreAttackable: false, IgnorePhase: true);
		if (combatTarget == null)
		{
			combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: true);
			if (combatTarget != null)
			{
				ParentObject.Fail("You cannot reach " + combatTarget.t() + " to attack " + combatTarget.them + ".");
			}
			else
			{
				ParentObject.Fail("There is nothing there you can attack.");
			}
			return false;
		}
		Event obj = Event.New("BeginAttack");
		obj.SetParameter("TargetObject", combatTarget);
		obj.SetParameter("TargetCell", combatTarget?.CurrentCell);
		if (!ParentObject.FireEvent(obj))
		{
			return false;
		}
		ParentObject.PlayWorldOrUISound("Sounds/Abilities/sfx_ability_skill_attack_generic_activate");
		int num = COOLDOWN;
		if (ParentObject.HasSkill("Multiweapon_Expertise"))
		{
			num -= 10;
		}
		if (ParentObject.HasSkill("Multiweapon_Mastery"))
		{
			num -= 10;
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, num, null, "Agility");
		DidX("launch", "into a flurry of attacks", "!", null, null, ParentObject);
		return Combat.PerformMeleeAttack(ParentObject, combatTarget, 1000, 0, 0, 0, "Flurrying");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandFlurry" && !PerformFlurry())
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Flurry", "CommandFlurry", "Skills", null, "ð", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
