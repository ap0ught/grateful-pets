using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_SmashUp : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public static readonly int COOLDOWN = 100;

	public static readonly int DURATION = 5;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Duration", DURATION);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CanRefreshAbilityEvent>.ID)
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
		if (E.Distance <= 1 && GameObject.Validate(E.Target) && IsPrimaryCudgelEquipped() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandSmashUp");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanRefreshAbilityEvent E)
	{
		if (E.Ability.ID == ActivatedAbilityID && ParentObject.HasEffect(typeof(Cudgel_SmashingUp)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerAfterDamage");
		Registrar.Register("CommandSmashUp");
		base.Register(Object, Registrar);
	}

	public bool IsPrimaryCudgelEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("Cudgel");
	}

	public GameObject GetPrimaryCudgel()
	{
		return ParentObject.GetPrimaryWeaponOfType("Cudgel");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandSmashUp")
		{
			if (!IsPrimaryCudgelEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a cudgel equipped in your primary hand to demolish things.");
				}
				return false;
			}
			Cudgel_Slam part = ParentObject.GetPart<Cudgel_Slam>();
			if (part != null && part.IsMyActivatedAbilityCoolingDown(part.ActivatedAbilityID))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't Demolish until Slam is off cooldown.");
				}
				return false;
			}
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("{{G|You prepare " + ParentObject.itself + " for demolition.}}");
				IComponent<GameObject>.ThePlayer.ParticleText("&R!!!");
			}
			int num = DURATION;
			if (ParentObject.HasIntProperty("ImprovedSmashUp"))
			{
				num += num * ParentObject.GetIntProperty("ImprovedSmashUp");
			}
			ParentObject.ApplyEffect(new Cudgel_SmashingUp(num));
			CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Demolish", "CommandSmashUp", "Stances", null, "\u001e");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
