using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Cudgel_Conk : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandConk";

	public static readonly int COOLDOWN = 10;

	public Guid ActivatedAbilityID = Guid.Empty;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != PooledEvent<CommandEvent>.ID)
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
		if (E.Distance <= 1 && GameObject.Validate(E.Target) && IsPrimaryCudgelEquipped() && E.Actor.CanMoveExtremities() && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && !PerformConk())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public GameObject GetPrimaryCudgel()
	{
		return ParentObject.GetPrimaryWeaponOfType("Cudgel");
	}

	public bool IsPrimaryCudgelEquipped()
	{
		return ParentObject.HasPrimaryWeaponOfType("Cudgel");
	}

	public string GetConkLocation(GameObject Object)
	{
		Body body = Object.Body;
		if (body == null)
		{
			return null;
		}
		int partCount = body.GetPartCount("Head");
		if (partCount <= 0)
		{
			return null;
		}
		if (partCount == 1)
		{
			return "the " + body.GetFirstPart("Head").Name;
		}
		List<string> list = new List<string>();
		foreach (BodyPart item2 in body.GetPart("Head"))
		{
			string item = Grammar.Pluralize(item2.VariantTypeModel().Name);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return "one of " + Object.its + " " + Grammar.MakeOrList(list);
	}

	public bool PerformConk()
	{
		if (!IsPrimaryCudgelEquipped())
		{
			ParentObject.Fail("You must have a cudgel equipped in your primary hand to conk.");
			return false;
		}
		if (!ParentObject.CanMoveExtremities("Conk", ShowMessage: true))
		{
			return false;
		}
		Cell cell = PickDirection("Conk");
		if (cell == null)
		{
			return false;
		}
		GameObject combatTarget = cell.GetCombatTarget(ParentObject);
		if (combatTarget == null)
		{
			ParentObject.Fail("There's nothing there you can conk.");
			return false;
		}
		try
		{
			string conkLocation = GetConkLocation(combatTarget);
			if (conkLocation == null)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail(combatTarget.Does("don't") + " have anything like a head to conk.");
				}
				return false;
			}
			if (combatTarget == ParentObject && ParentObject.IsPlayer() && Popup.ShowYesNo("Are you sure you want to conk " + combatTarget.itself + " on " + conkLocation + "?") != DialogResult.Yes)
			{
				return false;
			}
			DidXToY("attempt", "to conk", combatTarget, "on " + conkLocation);
			GameObject primaryCudgel = GetPrimaryCudgel();
			if (Combat.MeleeAttackWithWeapon(ParentObject, combatTarget, primaryCudgel, ParentObject.Body.FindDefaultOrEquippedItem(primaryCudgel), "Conking", 0, 0, 0, 0, 0, Primary: true).Hits > 0)
			{
				PlayWorldSound("sfx_ability_conk");
			}
			ParentObject.UseEnergy(1000, "Skill Cudgel Conk");
			CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
			return true;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Conk", x);
			return false;
		}
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Conk", COMMAND_NAME, "Skills", null, "-", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: true);
		return true;
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return true;
	}
}
