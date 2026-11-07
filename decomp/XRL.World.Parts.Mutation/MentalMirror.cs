using System;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class MentalMirror : BaseMutation
{
	[NonSerialized]
	public bool Active;

	public MentalMirror()
	{
		Type = "Mental";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("BonusMA", GetMABonus(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == BeforeMentalDefendEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("glass", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeMentalDefendEvent E)
	{
		if (!CheckActive() || E.Attacker == ParentObject)
		{
			return true;
		}
		if (E.Reflectable && E.Penetrations <= 0)
		{
			E.Penetrations = RollReflection(E);
			if (E.Penetrations > 0)
			{
				ReflectMessage(E.Attacker);
				E.Reflected = true;
				E.Defender = E.Attacker;
			}
		}
		else if (E.Penetrations > 0)
		{
			ShatterMessage(E.Attacker);
		}
		Activate();
		return base.HandleEvent(E);
	}

	public int RollReflection(IMentalAttackEvent E)
	{
		int combatMA = Stats.GetCombatMA(E.Attacker);
		int num = Math.Max(ParentObject.StatMod("Willpower"), Level);
		return Stat.RollPenetratingSuccesses("1d8", combatMA, num + E.Modifier);
	}

	public override string GetDescription()
	{
		return "You reflect mental attacks back at your attackers.";
	}

	public int GetMABonus(int Level)
	{
		return 3 + Level;
	}

	public int GetCooldown(int Level)
	{
		return 50;
	}

	public bool UpdateStatShifts()
	{
		if (Active)
		{
			StatShifter.SetStatShift("MA", GetMABonus(Level));
		}
		else
		{
			StatShifter.RemoveStatShifts();
		}
		return true;
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("When you suffer a mental attack while Mental Mirror is off cooldown, you gain +{{rules|" + GetMABonus(Level) + "}} mental armor (MA).\n", "If the attack then fails to penetrate your MA, it's reflected back at your attacker.\n"), "Cooldown: ", GetCooldown(Level).ToString());
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("DefenderAfterAttack");
		Registrar.Register("ReflectProjectile");
		base.Register(Object, Registrar);
	}

	public bool CheckActive()
	{
		bool flag = GetMyActivatedAbilityCooldown(ActivatedAbilityID) <= 0;
		if (flag != Active)
		{
			Active = flag;
			UpdateStatShifts();
		}
		return flag;
	}

	public void Activate()
	{
		CooldownMyActivatedAbility(ActivatedAbilityID, GetCooldown(Level));
	}

	public void ReflectMessage(GameObject Actor)
	{
		ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_mentalMirror_reflect");
		if (ParentObject.IsPlayer() || Actor.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("mental mirror reflects the attack!"));
		}
	}

	public void ShatterMessage(GameObject Actor)
	{
		if (ParentObject.IsPlayer() || Actor.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("mental mirror shatters!"));
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "DefenderAfterAttack" && CheckActive())
		{
			if (E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Mental"))
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				if (gameObjectParameter == null)
				{
					return true;
				}
				if (E.GetIntParameter("Penetrations") <= 0)
				{
					ReflectMessage(gameObjectParameter);
					GameObject gameObjectParameter2 = E.GetGameObjectParameter("Reflector");
					GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
					Combat.MeleeAttackWithWeapon(gameObjectParameter, gameObjectParameter2 ?? gameObjectParameter, gameObjectParameter3, gameObjectParameter3.DefaultOrEquippedPart(), E.GetStringParameter("Properties"), 0, 0, 0, MyPowerLoadBonus());
				}
				else
				{
					ShatterMessage(gameObjectParameter);
				}
				Activate();
			}
		}
		else if (E.ID == "ReflectProjectile" && CheckActive())
		{
			GameObject gameObjectParameter4 = E.GetGameObjectParameter("Projectile");
			if (gameObjectParameter4 != null && gameObjectParameter4.TryGetPart<Projectile>(out var Part) && Part.Attributes.Contains("Mental"))
			{
				Activate();
				if (80.in100())
				{
					float num = (float)E.GetParameter("Angle");
					E.SetParameter("Direction", (int)num + 180);
					E.SetParameter("By", ParentObject);
					E.SetParameter("Verb", "reflect");
					return false;
				}
				ShatterMessage(E.GetGameObjectParameter("Attacker"));
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		UpdateStatShifts();
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Mental Mirror", "CommandMentalMirror", "Mental Mutations", null, "m");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		StatShifter.RemoveStatShifts();
		return base.Unmutate(GO);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!Active)
		{
			CheckActive();
		}
	}
}
