using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class HydroTurbine : IPoweredPart
{
	public int MinimumEffectiveVolume = 1000;

	public int MaximumEffectiveVolume = 6000;

	public int MaximumChargeRate = 500;

	[NonSerialized]
	private int FoundVolume;

	public HydroTurbine()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		HydroTurbine hydroTurbine = p as HydroTurbine;
		if (hydroTurbine.MinimumEffectiveVolume != MinimumEffectiveVolume)
		{
			return false;
		}
		if (hydroTurbine.MaximumEffectiveVolume != MaximumEffectiveVolume)
		{
			return false;
		}
		if (hydroTurbine.MaximumChargeRate != MaximumChargeRate)
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
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int chargeRate = GetChargeRate();
			if (chargeRate > ChargeUse)
			{
				E.Amount += (chargeRate - ChargeUse) * E.Multiple;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (GetChargeRate() <= 0)
		{
			return true;
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "HydrodynamicForceInsufficient";
	}

	public int GetChargeRate()
	{
		int num = FindEffectiveNearbyLiquidVolume();
		if (num < MinimumEffectiveVolume)
		{
			return 0;
		}
		if (num >= MaximumEffectiveVolume)
		{
			return MaximumChargeRate;
		}
		num -= MinimumEffectiveVolume;
		return MaximumChargeRate * num / (MaximumEffectiveVolume - MinimumEffectiveVolume);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ProduceCharge(Amount);
	}

	private int FindEffectiveNearbyLiquidVolume()
	{
		FoundVolume = 0;
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && ScanForLiquidVolumes(cell))
		{
			List<Cell> localAdjacentCells = cell.GetLocalAdjacentCells();
			int i = 0;
			for (int count = localAdjacentCells.Count; i < count && ScanForLiquidVolumes(localAdjacentCells[i]); i++)
			{
			}
		}
		return FoundVolume;
	}

	private bool ScanForLiquidVolumes(Cell C)
	{
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			LiquidVolume liquidVolume = C.Objects[i].LiquidVolume;
			if (liquidVolume != null)
			{
				FoundVolume += liquidVolume.Volume;
				if (FoundVolume >= MaximumEffectiveVolume)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ProduceCharge(int Turns = 1)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
		{
			int chargeRate = GetChargeRate();
			if (chargeRate > ChargeUse)
			{
				ParentObject.ChargeAvailable(chargeRate - ChargeUse, 0L, Turns);
			}
		}
	}
}
