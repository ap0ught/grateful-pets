using System;
using XRL.Rules;

namespace XRL.World.Parts;

/// <remarks>
/// Adds elemental damage on <c>"WeaponHit"</c> when part is active. 
/// Uses the standard <see cref="T:XRL.World.Parts.IActivePart" /> interfaces, defaulting <see cref="!:WorksOnSelf" /> to true.
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is not by default, chance to activate is increased by a
/// percentage equal to ((power load - 100) / 10), i.e. 30% for
/// the standard overload power load of 400, and damage is increased
/// by the standard power load bonus, i.e. 2 for the standard overload
/// power load of 400.
/// </remarks>
[Serializable]
public class ElementalDamage : IActivePart
{
	public int Chance = 100;

	public string Damage = "1d4";

	public string Attributes = "Heat";

	private bool _WillActivateOnNextHit;

	public ElementalDamage()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		ElementalDamage elementalDamage = p as ElementalDamage;
		if (elementalDamage.Chance != Chance)
		{
			return false;
		}
		if (elementalDamage.Damage != Damage)
		{
			return false;
		}
		if (elementalDamage.Attributes != Attributes)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeforeMeleeAttackEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeMeleeAttackEvent E)
	{
		if (E.Weapon == ParentObject && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			GameObject actor = E.Actor;
			GameObject target = E.Target;
			GameObject weapon = E.Weapon;
			int effectiveChance = GetEffectiveChance(null, actor, target, weapon);
			_WillActivateOnNextHit = effectiveChance.in100();
			if (_WillActivateOnNextHit)
			{
				if (XRL.World.Damage.ContainsElectricDamage(Attributes))
				{
					PlayWorldSound("Sounds/Enhancements/sfx_enhancement_electric_attack", 0.5f, 0f, Combat: true);
				}
				if (XRL.World.Damage.ContainsColdDamage(Attributes))
				{
					PlayWorldSound("Sounds/Enhancements/sfx_enhancement_cold", 0.5f, 0f, Combat: true);
				}
				if (XRL.World.Damage.ContainsHeatDamage(Attributes))
				{
					PlayWorldSound("Sounds/Enhancements/sfx_enhancement_fire_attack", 0.5f, 0f, Combat: true);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			if (XRL.World.Damage.IsColdDamage(Attributes))
			{
				E.Add("ice", 1);
			}
			else if (XRL.World.Damage.IsElectricDamage(Attributes))
			{
				E.Add("circuitry", 1);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int num = MyPowerLoadBonus();
		string text = Damage;
		if (num != 0)
		{
			text = DieRoll.AdjustResult(text, num);
		}
		int effectiveChance = GetEffectiveChance();
		E.Postfix.AppendRules("This weapon deals " + text + " " + Attributes.ToLower() + " damage on hit" + ((effectiveChance < 100) ? (" " + effectiveChance + "% of the time") : "") + ".");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AdjustWeaponScore");
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			int num = MyPowerLoadLevel();
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
			if (_WillActivateOnNextHit)
			{
				int? powerLoadLevel = num;
				if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
				{
					string message = ((gameObjectParameter3 != null && !IComponent<GameObject>.TerseMessages) ? ("from " + gameObjectParameter.poss(gameObjectParameter3) + ".") : "from %t attack.");
					int amount = Damage.RollCached() + IComponent<GameObject>.PowerLoadBonus(num);
					string attributes = Attributes;
					GameObject owner = gameObjectParameter;
					GameObject source = gameObjectParameter3;
					string showDamageType = Attributes.ToLower() + " damage";
					if (gameObjectParameter2.TakeDamage(amount, message, attributes, null, null, owner, null, source, null, null, Accidental: false, Environmental: false, Indirect: false, ShowUninvolved: false, IgnoreVisibility: false, ShowForInanimate: false, SilentIfNoDamage: false, NoSetTarget: false, UsePopups: false, 5, showDamageType))
					{
						E.SetFlag("DidSpecialEffect", State: true);
					}
					if (!string.IsNullOrEmpty(Attributes))
					{
						if (Attributes.Contains("Umbral"))
						{
							gameObjectParameter2.ParticleBlip("&K-", 10, 0L);
							gameObjectParameter2.Acidsplatter();
						}
						if (XRL.World.Damage.ContainsAcidDamage(Attributes))
						{
							gameObjectParameter2.ParticleBlip("&G*", 10, 0L);
							gameObjectParameter2.Acidsplatter();
						}
						if (XRL.World.Damage.ContainsElectricDamage(Attributes))
						{
							gameObjectParameter2.ParticleBlip("&W*", 10, 0L);
							gameObjectParameter2.Sparksplatter();
						}
						if (XRL.World.Damage.ContainsColdDamage(Attributes))
						{
							gameObjectParameter2.ParticleBlip("&C*", 10, 0L);
							gameObjectParameter2.Icesplatter();
						}
						if (XRL.World.Damage.ContainsHeatDamage(Attributes))
						{
							gameObjectParameter2.ParticleBlip("&C*", 10, 0L);
							gameObjectParameter2.Firesplatter();
						}
					}
				}
			}
		}
		else if (E.ID == "AdjustWeaponScore" && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num2 = GetSpecialEffectChanceEvent.GetFor(E.GetGameObjectParameter("User"), ParentObject, "Part ElementalDamage Activation", Chance);
			int num3 = MyPowerLoadBonus();
			int num4 = ((Damage.RollMinCached() + num3) * 2 + (Damage.RollMaxCached() + num3)) * 2;
			if (num2 < 100)
			{
				num4 = num4 * num2 / 100;
			}
			E.SetParameter("Score", E.GetIntParameter("Score") + num4);
		}
		return base.FireEvent(E);
	}

	public int GetEffectiveChance(int? PowerLoadLevel = null, GameObject Attacker = null, GameObject Defender = null, GameObject Weapon = null)
	{
		int num = Chance;
		int num2 = IComponent<GameObject>.PowerLoadBonus(PowerLoadLevel ?? MyPowerLoadLevel(), 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return GetSpecialEffectChanceEvent.GetFor(Attacker, Weapon, "Part ElementalDamage Activation", num, Defender);
	}
}
