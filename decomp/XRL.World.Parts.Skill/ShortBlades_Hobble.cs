using System;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class ShortBlades_Hobble : BaseSkill
{
	public bool Hobbling;

	public Guid ActivatedAbilityID = Guid.Empty;

	public static readonly int Cooldown = 30;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("HobbleEffect", "-50% move speed");
		stats.Set("HobbleDuration", "16-20");
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), Cooldown);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 1 && E.Actor.CanMoveExtremities("Hobble") && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandHobble");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerAfterDamage");
		Registrar.Register("CommandHobble");
		base.Register(Object, Registrar);
	}

	public GameObject GetPrimaryShortblade()
	{
		return ParentObject.GetPrimaryWeaponOfType("ShortBlades");
	}

	public bool IsShortbladeEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("ShortBlades");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerAfterDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Weapon");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Defender");
			if (gameObjectParameter2 != null && Hobbling)
			{
				if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You find a weakness in " + Grammar.MakePossessive(gameObjectParameter3.the + gameObjectParameter3.ShortDisplayName) + " defenses.", 'g');
				}
				else if (gameObjectParameter3.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage(gameObjectParameter.The + gameObjectParameter.ShortDisplayName + gameObjectParameter.GetVerb("find") + " a weakness in your defenses.", 'r');
				}
				gameObjectParameter3.ApplyEffect(new Hobbled(Stat.Random(16, 20)));
			}
		}
		else if (E.ID == "CommandHobble")
		{
			if (!IsShortbladeEquipped())
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You must have a short blade equipped in your primary hand to hobble.");
				}
				return false;
			}
			if (!ParentObject.CanMoveExtremities("Hobble", ShowMessage: true))
			{
				return false;
			}
			Cell cell = PickDirection("Hobble");
			if (cell == null)
			{
				return false;
			}
			GameObject combatTarget = cell.GetCombatTarget(ParentObject);
			if (combatTarget == null)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("There's nothing there to hobble.");
				}
				return false;
			}
			try
			{
				Hobbling = true;
				if (combatTarget == ParentObject && ParentObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to hobble " + combatTarget.itself + "?") != DialogResult.Yes)
				{
					return false;
				}
				ParentObject.PlayWorldOrUISound("Sounds/Abilities/sfx_ability_skill_attack_generic_activate");
				DidXToY("attempt", "to hobble", combatTarget, null, null, null, null, null, combatTarget);
				GameObject primaryShortblade = GetPrimaryShortblade();
				Combat.MeleeAttackWithWeapon(ParentObject, combatTarget, primaryShortblade, ParentObject.Body.FindDefaultOrEquippedItem(primaryShortblade), "Autopen,Maxpen1", 0, 0, 0, 0, 0, Primary: true);
				int num = 1000;
				if (IsShortbladeEquipped() && ParentObject.HasSkill("ShortBlades_Expertise"))
				{
					num = (int)(0.75 * (double)num);
				}
				ParentObject.UseEnergy(num);
				CooldownMyActivatedAbility(ActivatedAbilityID, Cooldown, null, "Agility");
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Hobble", x);
			}
			finally
			{
				Hobbling = false;
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Hobble", "CommandHobble", "Skills", null, "-", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
