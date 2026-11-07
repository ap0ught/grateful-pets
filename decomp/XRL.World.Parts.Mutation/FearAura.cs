using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FearAura : BaseMutation
{
	public static readonly string COMMAND_NAME = "CommandFearAura";

	public int Chance = 25;

	public int EnergyCost = 1000;

	public int SpinTimer;

	public int Cooldown;

	public override string GetDescription()
	{
		return "You scare creatures around you.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat("You cause adjacent creatures to flee in terror.\n" + "Cooldown: " + GetCooldown(Level) + " rounds", "Additionally, you sometimes scare creatures passively.");
	}

	public int GetCooldown(int Level)
	{
		return 40;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID)
		{
			return ID == PooledEvent<CommandEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (ParentObject?.CurrentZone?.IsWorldMap() == true)
		{
			return true;
		}
		Cooldown--;
		if (Cooldown < 0 && IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID) && Chance.in100())
		{
			if (!PulseAura(ParentObject))
			{
				return false;
			}
			Cooldown = Stat.Random(21, 26);
		}
		if (EnergyCost > 0 && !ParentObject.IsPlayer())
		{
			UseEnergy(EnergyCost);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME && IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
		{
			if (PulseAura(ParentObject) && EnergyCost > 0 && !ParentObject.IsPlayer())
			{
				UseEnergy(EnergyCost);
			}
			CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
			DidX("assume", "a menacing pose", null, null, null, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public static bool PulseAura(GameObject Actor)
	{
		List<Cell> list = Actor?.CurrentCell?.GetLocalAdjacentCells();
		if (list == null)
		{
			return false;
		}
		bool flag = false;
		foreach (Cell item in list)
		{
			foreach (GameObject @object in item.Objects)
			{
				if (@object.Brain != null)
				{
					Mental.PerformAttack(ApplyFear, Actor, @object, null, "Terrify Aura", "1d8+4", 8388609, "1d3".RollCached());
					flag = true;
				}
			}
		}
		if (flag)
		{
			Actor.ParticleBlip("&W!", 10, 0L);
		}
		return flag;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static bool ApplyFear(MentalAttackEvent E)
	{
		if (!E.Defender.FireEvent("CanApplyFear"))
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check<Terrified>(E.Defender))
		{
			return false;
		}
		if (E.Penetrations > 0)
		{
			Terrified e = new Terrified(E.Magnitude, E.Attacker, Psionic: true);
			if (E.Defender.ApplyEffect(e))
			{
				return true;
			}
		}
		if (E.Defender.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You feel uneasy.", 'K');
		}
		return false;
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Fear Aura", COMMAND_NAME, "Physical Mutations", null, "®");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
