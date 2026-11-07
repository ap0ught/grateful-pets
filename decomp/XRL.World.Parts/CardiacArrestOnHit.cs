using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// <remarks>
/// This part is not used in the base game.
///
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is not by default, chance to activate is increased by a
/// percentage equal to ((power load - 100) / 10), i.e. 30% for
/// the standard overload power load of 400, and save targets are
/// increased by the standard power load bonus, i.e. 2 for the standard
/// overload power load of 400.
/// </remarks>
[Serializable]
public class CardiacArrestOnHit : IActivePart
{
	public int Chance = 100;

	public int SaveTarget = 20;

	public string SaveAttribute = "Toughness";

	public string SaveVs = "CardiacArrest CardiacArrestInduction";

	public string RequireDamageAttribute;

	public CardiacArrestOnHit()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		CardiacArrestOnHit cardiacArrestOnHit = p as CardiacArrestOnHit;
		if (cardiacArrestOnHit.Chance != Chance)
		{
			return false;
		}
		if (cardiacArrestOnHit.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (cardiacArrestOnHit.SaveAttribute != SaveAttribute)
		{
			return false;
		}
		if (cardiacArrestOnHit.SaveVs != SaveVs)
		{
			return false;
		}
		if (cardiacArrestOnHit.RequireDamageAttribute != RequireDamageAttribute)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LauncherProjectileHit");
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponDealDamage");
		Registrar.Register("WeaponPseudoThrowHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public int GetActivationChance(GameObject Attacker = null, GameObject Defender = null, GameObject Projectile = null, int? PowerLoadLevel = null)
	{
		int num = Chance;
		int num2 = IComponent<GameObject>.PowerLoadBonus(PowerLoadLevel ?? MyPowerLoadLevel(), 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return GetSpecialEffectChanceEvent.GetFor(Attacker, ParentObject, "Part CardiacArrestOnHit Activation", num, Defender, Projectile);
	}

	public int GetSaveTarget(int? PowerLoadLevel = null)
	{
		return SaveTarget + IComponent<GameObject>.PowerLoadBonus(PowerLoadLevel ?? MyPowerLoadLevel());
	}

	public void CheckApply(Event E)
	{
		if (!RequireDamageAttribute.IsNullOrEmpty() && (!(E.GetParameter("Damage") is Damage damage) || !damage.HasAttribute(RequireDamageAttribute)))
		{
			return;
		}
		int value = MyPowerLoadLevel();
		int? powerLoadLevel = value;
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return;
		}
		GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
		GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
		GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
		if (GetActivationChance(gameObjectParameter, gameObjectParameter2, gameObjectParameter3, value).in100())
		{
			powerLoadLevel = value;
			ConsumeCharge(null, powerLoadLevel);
			if (CanApplyEffectEvent.Check<CardiacArrest>(gameObjectParameter2) && (SaveAttribute.IsNullOrEmpty() || !gameObjectParameter2.MakeSave(SaveAttribute, GetSaveTarget(value), null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject)))
			{
				gameObjectParameter2.ApplyEffect(new CardiacArrest());
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage" || E.ID == "ProjectileHit" || E.ID == "LauncherProjectileHit" || E.ID == "WeaponThrowHit" || E.ID == "WeaponPseudoThrowHit")
		{
			CheckApply(E);
		}
		return base.FireEvent(E);
	}
}
