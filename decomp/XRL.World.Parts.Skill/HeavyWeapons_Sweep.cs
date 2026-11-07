using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Skill;

[Serializable]
public class HeavyWeapons_Sweep : BaseSkill
{
	public static readonly string COMMAND_NAME = "CommandHeavyWeaponsSweep";

	public static readonly int BASE_SHOTS = 5;

	public static readonly int BASE_WIDTH = 90;

	public static readonly int COOLDOWN = 250;

	public Guid ActivatedAbilityID = Guid.Empty;

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.Set("Shots", BASE_SHOTS);
		stats.Set("ConeWidth", BASE_WIDTH);
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
		if (E.Actor == ParentObject && E.Distance < 8 && GameObject.Validate(E.Target) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && E.Actor.CanMoveExtremities(null, ShowMessage: false, Involuntary: false, AllowTelekinetic: true) && !E.Target.HasAdjacentAllyOf(E.Actor) && E.Distance < Stat.Random(3, 8) && E.Actor.HasMissileWeapon(null, (MissileWeapon mw) =>
		{
			if (!IsHeavyWeapon(mw))
			{
				return false;
			}
			if (!mw.ReadyToFire())
			{
				return false;
			}
			int num = mw.AmmoPerAction * GetShots(E.Actor);
			if (num > 0)
			{
				int num2 = GetAmmoCountAvailableEvent.GetFor(mw.ParentObject, mw);
				if (num2 > 0 && Stat.Random(mw.AmmoPerAction * 2, num) > num2)
				{
					return false;
				}
			}
			return true;
		}))
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && !PerformSweep(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Sweep", COMMAND_NAME, "Skills", null, "®");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}

	public static bool IsHeavyWeapon(MissileWeapon MW)
	{
		return MW?.Skill == "HeavyWeapons";
	}

	public static bool IsHeavyWeapon(GameObject Object)
	{
		return IsHeavyWeapon(Object.GetPart<MissileWeapon>());
	}

	public bool PerformSweep(GameObject Actor = null)
	{
		if (Actor == null)
		{
			Actor = ParentObject;
		}
		if (!GameObject.Validate(ref Actor))
		{
			return false;
		}
		if (!ParentObject.CanMoveExtremities(null, ShowMessage: true, Involuntary: false, AllowTelekinetic: true))
		{
			return false;
		}
		bool flag = false;
		string text = null;
		GameObject gameObject = null;
		List<GameObject> missileWeapons = ParentObject.GetMissileWeapons(null, IsHeavyWeapon);
		if (missileWeapons == null || missileWeapons.Count <= 0)
		{
			return ParentObject.Fail("You do not have a heavy missile weapon equipped.");
		}
		ParentObject.PlayWorldOrUISound("Sounds/Abilities/sfx_ability_skill_attack_generic_activate");
		foreach (GameObject item in missileWeapons)
		{
			MissileWeapon part = item.GetPart<MissileWeapon>();
			if (!part.ReadyToFire() && Actor != null)
			{
				CommandReloadEvent.Execute(Actor, FreeAction: true);
			}
			if (part.ReadyToFire())
			{
				flag = true;
				break;
			}
			if (text == null)
			{
				text = part.GetNotReadyToFireMessage();
				gameObject = item;
			}
		}
		if (!flag)
		{
			if (ParentObject.IsPlayer())
			{
				SoundManager.PlaySound(gameObject?.GetSoundTag("NoAmmoSound"));
			}
			return ParentObject.Fail(text ?? ("You need to reload! (" + ControlManager.getCommandInputDescription("CmdReload", Options.ModernUI) + ")"));
		}
		if (!Combat.FireMissileWeapon(ParentObject, null, null, FireType.Normal, "HeavyWeapons", 0, GetShots(Actor), GetWidth(Actor)))
		{
			return false;
		}
		Actor.UseEnergy(1000, "Physical Skill HeavyWeapons Sweep");
		CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
		return true;
	}

	public static int GetShots(GameObject Actor)
	{
		return BASE_SHOTS + (Actor?.GetIntProperty("HeavyWeaponsSweepModifyShots") ?? 0);
	}

	public int GetShots()
	{
		return GetShots(ParentObject);
	}

	public static int GetWidth(GameObject Actor)
	{
		return BASE_WIDTH + (Actor?.GetIntProperty("HeavyWeaponsSweepModifyWidth") ?? 0);
	}

	public int GetWidth()
	{
		return GetWidth(ParentObject);
	}
}
