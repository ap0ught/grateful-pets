using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, charge rate is increased by a percentage
/// equal to power load, i.e. 400% for the standard overload power load
/// of 400.
/// </remarks>
[Serializable]
public class ZeroPointEnergyCollector : IPoweredPart
{
	public int ChargeRate = 10;

	public string World = "JoppaWorld";

	public string Plane = "*";

	public List<string> Worlds;

	public ZeroPointEnergyCollector()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		IsPowerLoadSensitive = true;
		IsRealityDistortionBased = true;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		ZeroPointEnergyCollector zeroPointEnergyCollector = p as ZeroPointEnergyCollector;
		if (zeroPointEnergyCollector.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (zeroPointEnergyCollector.World != World)
		{
			return false;
		}
		if (zeroPointEnergyCollector.Plane != Plane)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != QueryChargeProductionEvent.ID)
		{
			return ID == PrimePowerSystemsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PrimePowerSystemsEvent E)
	{
		if (ParentObject.HasPropertyOrTag("Furniture"))
		{
			ProduceCharge();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeProductionEvent E)
	{
		if (ChargeRate > ChargeUse && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Amount += GetEffectiveChargeRate();
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		Zone anyBasisZone = GetAnyBasisZone();
		if (anyBasisZone == null)
		{
			return true;
		}
		if (World.IsNullOrEmpty())
		{
			World = anyBasisZone.ResolveZoneWorld();
			Worlds = World.CachedCommaExpansion();
		}
		else if (World != "*")
		{
			if (Worlds == null)
			{
				Worlds = World.CachedCommaExpansion();
			}
			if (!Worlds.Contains(anyBasisZone.ResolveZoneWorld()))
			{
				return true;
			}
		}
		if (Plane != "*" && anyBasisZone.ResolveWorldBlueprint().Plane != Plane)
		{
			return true;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "QuantumPhaseMismatch";
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ProduceCharge(Amount);
	}

	public int GetEffectiveChargeRate(int Multiple = 1)
	{
		int num = ChargeRate - ChargeUse;
		if (Multiple != 1)
		{
			num *= Multiple;
		}
		int num2 = MyPowerLoadLevel();
		if (num2 != 100)
		{
			num = num * num2 / 100;
		}
		return num;
	}

	public void ProduceCharge(int Turns = 1)
	{
		if (ChargeRate > ChargeUse && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
		{
			ParentObject.ChargeAvailable(GetEffectiveChargeRate(), 0L, Turns);
		}
	}
}
