using System;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class GroundOnHit : IPoweredPart
{
	public int Chance = 100;

	public int SaveTarget = 40;

	public string SaveStat = "Agility";

	public string SaveVs = "Grounding";

	public bool FlyingLevelAidsSave = true;

	public string Duration = "20-30";

	public float ComputePowerFactor = 1f;

	public GroundOnHit()
	{
		ChargeUse = 100;
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
		NameForStatus = "FlightInterdiction";
	}

	public override bool SameAs(IPart p)
	{
		GroundOnHit groundOnHit = p as GroundOnHit;
		if (groundOnHit.Chance != Chance)
		{
			return false;
		}
		if (groundOnHit.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (groundOnHit.SaveStat != SaveStat)
		{
			return false;
		}
		if (groundOnHit.SaveVs != SaveVs)
		{
			return false;
		}
		if (groundOnHit.FlyingLevelAidsSave != FlyingLevelAidsSave)
		{
			return false;
		}
		if (groundOnHit.Duration != Duration)
		{
			return false;
		}
		if (groundOnHit.ComputePowerFactor != ComputePowerFactor)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "WeaponThrowHit" || E.ID == "ProjectileHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			GameObject projectile = gameObjectParameter3;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part GroundOnHit Activation", Chance, subject, projectile).in100())
			{
				CheckGrounding(gameObjectParameter, gameObjectParameter2, E.ID == "ProjectileHit");
			}
		}
		return base.FireEvent(E);
	}

	public void CheckGrounding(GameObject who, GameObject target, bool asProjectile = false)
	{
		int num = ((!asProjectile) ? ParentObject : ParentObject.GetPart<Projectile>()?.Launcher)?.GetPowerLoadLevel() ?? 100;
		int? powerLoadLevel = num;
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			return;
		}
		int num2 = SaveTarget;
		if (FlyingLevelAidsSave)
		{
			int num3 = Flight.GetBestFlyingLevel(target);
			Wings part = target.GetPart<Wings>();
			if (part != null && part.Level > num3)
			{
				num3 = part.Level;
			}
			num2 -= num3;
		}
		if (target.MakeSave(SaveStat, num2, null, null, SaveVs))
		{
			return;
		}
		int num4 = GetAvailableComputePowerEvent.AdjustUp(who, Duration.RollCached() * (100 + IComponent<GameObject>.PowerLoadBonus(num, 100, 10)) / 100, ComputePowerFactor);
		Grounded grounded = target.GetEffect<Grounded>();
		if (grounded == null)
		{
			grounded = new Grounded();
			if (!target.ApplyEffect(grounded))
			{
				return;
			}
		}
		if (grounded.Duration < num4)
		{
			grounded.Duration = num4;
		}
	}
}
