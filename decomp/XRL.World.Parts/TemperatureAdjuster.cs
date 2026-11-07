using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is set to
/// true, which it is not by default, temperature changes are increased
/// in magnitude by a percentage equal to ((power load - 100) / 10), i.e.
/// 30% for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class TemperatureAdjuster : IPoweredPart
{
	public int TemperatureAmount = 5;

	public int TemperatureThreshold = 100;

	public bool ThresholdAbove = true;

	public bool AlwaysUseCharge;

	public string BehaviorDescription;

	public bool InactiveOnWorldMap;

	public TemperatureAdjuster()
	{
		WorksOnSelf = true;
		WorksOnHolder = true;
		WorksOnWearer = true;
		WorksOnCarrier = true;
		WorksOnCellContents = true;
		WorksOnAdjacentCellContents = true;
	}

	public override bool SameAs(IPart p)
	{
		TemperatureAdjuster temperatureAdjuster = p as TemperatureAdjuster;
		if (temperatureAdjuster.TemperatureAmount != TemperatureAmount)
		{
			return false;
		}
		if (temperatureAdjuster.TemperatureThreshold != TemperatureThreshold)
		{
			return false;
		}
		if (temperatureAdjuster.ThresholdAbove != ThresholdAbove)
		{
			return false;
		}
		if (temperatureAdjuster.AlwaysUseCharge != AlwaysUseCharge)
		{
			return false;
		}
		if (temperatureAdjuster.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		if (temperatureAdjuster.InactiveOnWorldMap != InactiveOnWorldMap)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (InactiveOnWorldMap)
		{
			if (ActivePartHasMultipleSubjects())
			{
				foreach (GameObject activePartSubject in GetActivePartSubjects())
				{
					if (activePartSubject.OnWorldMap())
					{
						return true;
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (activePartFirstSubject != null && activePartFirstSubject.OnWorldMap())
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		return "Inactive";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != GetShortDescriptionEvent.ID || BehaviorDescription.IsNullOrEmpty()) && (ID != SingletonEvent<RadiatesHeatAdjacentEvent>.ID || !WorksOnAdjacentCellContents || TemperatureAmount <= 0))
		{
			if (ID == SingletonEvent<RadiatesHeatEvent>.ID && WorksOnCellContents)
			{
				return TemperatureAmount > 0;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!BehaviorDescription.IsNullOrEmpty())
		{
			E.Postfix.AppendRules(BehaviorDescription, GetEventSensitiveAddStatusSummary(E));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatAdjacentEvent E)
	{
		if (WorksOnAdjacentCellContents && TemperatureAmount > 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RadiatesHeatEvent E)
	{
		if (WorksOnCellContents && TemperatureAmount > 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ProcessTurns(Amount);
	}

	public int ProcessTurns(int Turns)
	{
		int num = 0;
		int num2 = MyPowerLoadLevel();
		bool alwaysUseCharge = AlwaysUseCharge;
		int? powerLoadLevel = num2;
		if (IsReady(alwaysUseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			bool flag = false;
			if (ActivePartHasMultipleSubjects())
			{
				foreach (GameObject activePartSubject in GetActivePartSubjects())
				{
					for (int i = 1; i <= Turns; i++)
					{
						if (!AdjustTemperature(activePartSubject, num2))
						{
							break;
						}
						flag = true;
						if (i > num)
						{
							num = i;
						}
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (activePartFirstSubject != null)
				{
					for (int j = 1; j <= Turns; j++)
					{
						if (!AdjustTemperature(activePartFirstSubject, num2))
						{
							break;
						}
						flag = true;
						if (j > num)
						{
							num = j;
						}
					}
				}
			}
			if (flag && !AlwaysUseCharge)
			{
				int multipleCharge = num;
				powerLoadLevel = num2;
				ConsumeCharge(multipleCharge, null, powerLoadLevel);
			}
		}
		return num;
	}

	public bool AdjustTemperature(GameObject obj, int PowerLoad = int.MinValue)
	{
		if (obj.Physics == null)
		{
			return false;
		}
		if (ThresholdAbove)
		{
			if (obj.Physics.Temperature >= TemperatureThreshold)
			{
				return false;
			}
		}
		else if (obj.Physics.Temperature <= TemperatureThreshold)
		{
			return false;
		}
		int num = TemperatureAmount;
		if (IsPowerLoadSensitive)
		{
			if (PowerLoad == int.MinValue)
			{
				PowerLoad = MyPowerLoadLevel();
			}
			int num2 = MyPowerLoadBonus(PowerLoad, 100, 10);
			if (num2 != 0)
			{
				num = num * (100 + num2) / 100;
			}
		}
		obj.TemperatureChange(num, obj.Equipped ?? obj);
		return true;
	}
}
