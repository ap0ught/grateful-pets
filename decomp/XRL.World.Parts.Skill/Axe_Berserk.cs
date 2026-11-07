using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Axe_Berserk : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public static readonly int COOLDOWN = 100;

	public static readonly int DURATION = 5;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Duration", GetDuration());
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != PooledEvent<CanRefreshAbilityEvent>.ID)
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
		if (IsPrimaryAxeEquipped() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && GameObject.Validate(E.Target) && ConTarget(E.Target) >= 1f)
		{
			E.Add("CommandAxeBerserk");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanRefreshAbilityEvent E)
	{
		if (E.Ability.ID == ActivatedAbilityID && ParentObject.HasEffect(typeof(Berserk)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandAxeBerserk");
		base.Register(Object, Registrar);
	}

	public bool IsPrimaryAxeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("Axe");
	}

	public GameObject GetPrimaryAxe()
	{
		return ParentObject.GetPrimaryWeaponOfType("Axe");
	}

	public int GetDuration()
	{
		int num = DURATION;
		if (ParentObject.HasIntProperty("ImprovedBerserk"))
		{
			num += num * ParentObject.GetIntProperty("ImprovedBerserk");
		}
		return num;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandAxeBerserk")
		{
			if (!IsPrimaryAxeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have an axe equipped in your primary hand to go berserk.");
				}
				return false;
			}
			Axe_Dismember part = ParentObject.GetPart<Axe_Dismember>();
			if (part != null && part.IsMyActivatedAbilityCoolingDown(part.ActivatedAbilityID))
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You can't go berserk until Dismember is off cooldown.");
				}
				return false;
			}
			IComponent<GameObject>.XDidY(ParentObject, "work", ParentObject.itself + " into a blood frenzy", "!", null, null, ParentObject);
			ParentObject.ParticleText("&R!!!");
			ParentObject.ApplyEffect(new Berserk(GetDuration()));
			CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
			part?.CooldownMyActivatedAbility(part.ActivatedAbilityID, Axe_Dismember.Cooldown);
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Berserk!", "CommandAxeBerserk", "Stances", null, "\u0001");
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
