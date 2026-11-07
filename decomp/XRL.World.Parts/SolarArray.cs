using System;

namespace XRL.World.Parts;

[Serializable]
public class SolarArray : IPoweredPart
{
	public int ChargeRate = 10;

	public SolarArray()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as SolarArray).ChargeRate != ChargeRate)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != QueryChargeProductionEvent.ID)
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

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "ChargeRate", ChargeRate);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeProductionEvent E)
	{
		if (ChargeRate > ChargeUse && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Amount += (ChargeRate - ChargeUse) * E.Multiple;
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		Cell anyBasisCell = GetAnyBasisCell();
		if (anyBasisCell == null)
		{
			return true;
		}
		if (anyBasisCell.IsBlackedOut())
		{
			return true;
		}
		Zone parentZone = anyBasisCell.ParentZone;
		if (parentZone == null)
		{
			return true;
		}
		if (!parentZone.IsWorldMap() && !parentZone.IsOutside())
		{
			return true;
		}
		if (!IsDay())
		{
			return true;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "RadiationFluxInsufficient";
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ProduceCharge(Amount);
	}

	public void ProduceCharge(int Turns = 1)
	{
		if (ChargeRate > ChargeUse && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			ParentObject.ChargeAvailable(ChargeRate - ChargeUse, 0L, Turns);
		}
	}
}
