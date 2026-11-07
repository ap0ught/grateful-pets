using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class EmitGasOnHit : IActivePart
{
	public int Chance = 100;

	public int ChanceEach = 100;

	public string GasBlueprint = "PoisonGas";

	public string CellDensity = "4d10";

	public string AdjacentDensity = "2d10";

	public string GasLevel = "1";

	public string BehaviorDescription;

	public EmitGasOnHit()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		EmitGasOnHit emitGasOnHit = p as EmitGasOnHit;
		if (emitGasOnHit.Chance != Chance)
		{
			return false;
		}
		if (emitGasOnHit.ChanceEach != ChanceEach)
		{
			return false;
		}
		if (emitGasOnHit.GasBlueprint != GasBlueprint)
		{
			return false;
		}
		if (emitGasOnHit.CellDensity != CellDensity)
		{
			return false;
		}
		if (emitGasOnHit.AdjacentDensity != AdjacentDensity)
		{
			return false;
		}
		if (emitGasOnHit.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return !string.IsNullOrEmpty(BehaviorDescription);
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(BehaviorDescription))
		{
			E.Postfix.AppendRules(BehaviorDescription);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		Registrar.Register("ProjectileHit");
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileHit" || E.ID == "WeaponHit" || E.ID == "AttackerHit" || E.ID == "WeaponThrowHit")
		{
			CheckApply(E);
		}
		return base.FireEvent(E);
	}

	public int CheckApply(Event E)
	{
		if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return 0;
		}
		GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
		GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
		GameObject gameObjectParameter3 = E.GetGameObjectParameter("Weapon");
		GameObject gameObjectParameter4 = E.GetGameObjectParameter("Projectile");
		Cell impactCell = E.GetParameter("ImpactCell") as Cell;
		GameObject parentObject = ParentObject;
		GameObject subject = gameObjectParameter2;
		GameObject projectile = gameObjectParameter4;
		if (!GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part EmitGasOnHit Activation Main", Chance, subject, projectile).in100())
		{
			return 0;
		}
		GameObject parentObject2 = ParentObject;
		projectile = gameObjectParameter2;
		subject = gameObjectParameter4;
		int useChanceEach = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject2, "Part EmitGasOnHit Activation Each", ChanceEach, projectile, subject);
		GameObject gameObject = gameObjectParameter4 ?? gameObjectParameter3 ?? gameObjectParameter;
		bool phased = gameObject?.HasEffect<Phased>() ?? false;
		bool omniphase = gameObject?.HasEffect<Omniphase>() ?? false;
		return EmitGas(gameObjectParameter, gameObjectParameter2, impactCell, useChanceEach, phased, omniphase);
	}

	public int EmitGas(GameObject Creator, GameObject Object, Cell ImpactCell, int UseChanceEach, bool Phased, bool Omniphase)
	{
		int result = 0;
		Cell cell = Object?.CurrentCell;
		if (cell == null)
		{
			cell = ImpactCell;
		}
		if (cell != null)
		{
			Event e = Event.New("CreatorModifyGas", "Gas", (object)null);
			EmitGas(cell, Creator, e, CellDensity.RollCached(), GasLevel.RollCached(), UseChanceEach, Phased, Omniphase);
			foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
			{
				EmitGas(localAdjacentCell, Creator, e, AdjacentDensity.RollCached(), GasLevel.RollCached(), UseChanceEach, Phased, Omniphase);
			}
		}
		return result;
	}

	public bool EmitGas(Cell C, GameObject Creator, Event E, int Density, int Level, int UseChanceEach, bool Phased, bool Omniphase)
	{
		if (!UseChanceEach.in100())
		{
			return false;
		}
		GameObject firstObject = C.GetFirstObject(GasBlueprint);
		if (firstObject == null)
		{
			firstObject = GameObject.Create(GasBlueprint);
			Gas part = firstObject.GetPart<Gas>();
			part.Density = Density;
			part.Level = Level;
			part.Creator = Creator;
			if (Phased)
			{
				firstObject.ForceApplyEffect(new Phased(Stat.Random(23, 32)));
			}
			if (Omniphase)
			{
				firstObject.ForceApplyEffect(new Omniphase(Stat.Random(46, 64)));
			}
			E.SetParameter("Gas", part);
			Creator?.FireEvent(E);
			C.AddObject(firstObject);
		}
		else
		{
			Gas part2 = firstObject.GetPart<Gas>();
			part2.Density += Density;
			if (part2.Level < Level || part2.Density < Density * 2)
			{
				part2.Level = Level;
			}
		}
		return true;
	}
}
