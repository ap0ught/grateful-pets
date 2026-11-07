using System;

namespace XRL.World.Parts;

[Serializable]
public class SlottedCellCharger : IPoweredPart
{
	public int ChargeRate;

	public long ActiveTurn;

	public int UsedOnTurn;

	public SlottedCellCharger()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
		NameForStatus = "ChargingSystem";
	}

	public GameObject GetChargeableCell()
	{
		if (ParentObject.TryGetPart<EnergyCellSocket>(out var Part) && Part.Cell != null && Part.Cell.TryGetPart<EnergyCell>(out var Part2) && Part2.Charge < Part2.MaxCharge)
		{
			return Part.Cell;
		}
		return null;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		return GetChargeableCell() == null;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		EnergyCellSocket part = ParentObject.GetPart<EnergyCellSocket>();
		if (part == null)
		{
			return "NoSocket";
		}
		if (part.Cell == null)
		{
			return "NoCell";
		}
		EnergyCell part2 = part.Cell.GetPart<EnergyCell>();
		if (part2 == null)
		{
			return "InappropriateCell";
		}
		if (part2.Charge >= part2.MaxCharge)
		{
			return "CellFull";
		}
		return "Error";
	}

	public override bool SameAs(IPart p)
	{
		if ((p as SlottedCellCharger).ChargeRate != ChargeRate)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ChargeAvailableEvent.ID)
		{
			return ID == QueryChargeStorageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ChargeAvailableEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CheckTurn();
			GameObject chargeableCell = GetChargeableCell();
			if (chargeableCell != null)
			{
				int num = ((ChargeRate == 0 || E.Forced) ? (E.Amount - ChargeUse * E.Multiple) : Math.Min(ChargeRate * E.Multiple - UsedOnTurn, E.Amount - ChargeUse * E.Multiple));
				if (num > 0)
				{
					int num2 = RechargeAvailableEvent.Send(chargeableCell, E, num);
					if (num2 != 0)
					{
						if (E.Multiple > 1)
						{
							UsedOnTurn = num2 / E.Multiple;
						}
						else
						{
							UsedOnTurn += num2;
						}
						E.Amount -= num2 + ChargeUse * E.Multiple;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			CheckTurn();
			if (ChargeRate <= 0 || ChargeRate > UsedOnTurn)
			{
				GameObject chargeableCell = GetChargeableCell();
				if (chargeableCell != null)
				{
					int num = QueryRechargeStorageEvent.Retrieve(chargeableCell, E);
					if (num > 0)
					{
						if (ChargeRate > 0)
						{
							num = Math.Min(num, ChargeRate - UsedOnTurn);
						}
						E.Amount += num;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public void CheckTurn()
	{
		if (ActiveTurn < The.CurrentTurn)
		{
			UsedOnTurn = 0;
			ActiveTurn = The.CurrentTurn;
		}
	}
}
