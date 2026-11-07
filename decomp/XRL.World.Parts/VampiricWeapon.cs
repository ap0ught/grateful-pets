using System;
using System.Text;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class VampiricWeapon : IActivePart
{
	public int Chance = 100;

	public string Percent;

	public string Reduction;

	public string Maximum;

	public bool WorksInMelee = true;

	public bool WorksThrown = true;

	public bool WorksAsProjectile = true;

	public bool WorksAsAttacker = true;

	public bool RealityDistortionBased;

	public bool RequiresLivingTarget;

	public VampiricWeapon()
	{
		WorksOnSelf = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AttackerDealtDamageEvent.ID || !WorksAsAttacker))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AttackerDealtDamageEvent E)
	{
		if (WorksAsAttacker)
		{
			Absorb(E.Damage, E.Actor, E.Object, E.Weapon, E.Projectile, E);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(AppendEffect);
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		SB.Append("Heal for ");
		SB.Append(Percent.IsNullOrEmpty() ? "100" : Percent);
		SB.Append("% of damage dealt");
		if (RequiresLivingTarget)
		{
			SB.Append(" vs. organic creatures");
		}
		if (!Reduction.IsNullOrEmpty())
		{
			SB.Append(", -").Append(Reduction);
		}
		if (!Maximum.IsNullOrEmpty())
		{
			SB.Append(", max ").Append(Maximum);
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (WorksInMelee)
		{
			Registrar.Register("WeaponDealDamage");
		}
		if (WorksThrown)
		{
			Registrar.Register("WeaponThrowHit");
		}
		if (WorksAsProjectile)
		{
			Registrar.Register("ProjectileHit");
		}
	}

	public override bool FireEvent(Event E)
	{
		if ((WorksInMelee && E.ID == "WeaponDealDamage") || (WorksThrown && E.ID == "WeaponThrowHit") || (WorksAsProjectile && E.ID == "ProjectileHit"))
		{
			Absorb(E.GetParameter("Damage") as Damage, E.GetGameObjectParameter("Attacker"), E.GetGameObjectParameter("Defender"), E.GetGameObjectParameter("Weapon"), E.GetGameObjectParameter("Projectile"), E);
		}
		return base.FireEvent(E);
	}

	public void Absorb(Damage Damage, GameObject Attacker, GameObject Defender, GameObject Weapon, GameObject Projectile, IEvent ParentEvent)
	{
		if (!Attacker.IsValid() || Damage == null || Damage.Amount <= 0 || (RequiresLivingTarget && !Defender.IsAlive))
		{
			return;
		}
		int chance = Chance;
		if (WorksInMelee || WorksThrown || WorksAsProjectile)
		{
			chance = GetSpecialEffectChanceEvent.GetFor(Attacker, Weapon, "Part VampiricWeapon Activation", Chance, Defender, Projectile);
		}
		if (!chance.in100())
		{
			return;
		}
		if (RealityDistortionBased)
		{
			Cell cell = Defender.CurrentCell;
			Event obj = Event.New("InitiateRealityDistortionTransit");
			obj.SetParameter("Object", Attacker);
			obj.SetParameter("Device", Weapon ?? ParentObject);
			obj.SetParameter("Operator", Attacker);
			obj.SetParameter("Cell", cell);
			if ((Attacker.IsValid() && !Attacker.FireEvent(obj, ParentEvent)) || (cell != null && !cell.FireEvent(obj, ParentEvent)))
			{
				return;
			}
		}
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = Damage.Amount;
			if (!string.IsNullOrEmpty(Percent))
			{
				num = num * Stat.Roll(Percent) / 100;
			}
			if (!string.IsNullOrEmpty(Reduction))
			{
				num -= Stat.Roll(Reduction);
			}
			if (!string.IsNullOrEmpty(Maximum))
			{
				num = Math.Min(num, Stat.Roll(Maximum));
			}
			if (num > 0)
			{
				Attacker.Heal(num, Message: true, FloatText: true);
			}
		}
	}
}
