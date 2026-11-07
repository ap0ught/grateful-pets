using System;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, compute power output is increased by a
/// percentage equal to ((power load - 100) / 10), i.e. 30% for
/// the standard overload power load of 400.
/// </remarks>
[Serializable]
public class ComputeNode : IPoweredPart
{
	public int Power = 20;

	public ComputeNode()
	{
		WorksOnSelf = true;
		IsPowerLoadSensitive = true;
	}

	public override bool WantTurnTick()
	{
		return ChargeUse > 0;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!OnWorldMap)
		{
			ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetAvailableComputePowerEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetAvailableComputePowerEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Amount += GetEffectivePower();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Power != 0 && (WorksOnEquipper || WorksOnWearer || WorksOnHolder || WorksOnImplantee))
		{
			E.Postfix.Append("\n{{rules|When ");
			if (WorksOnEquipper || WorksOnWearer || WorksOnHolder)
			{
				if (WorksOnImplantee)
				{
					E.Postfix.Append("equipped or implanted");
				}
				else
				{
					E.Postfix.Append("equipped");
				}
			}
			else if (WorksOnImplantee)
			{
				E.Postfix.Append("implanted");
			}
			if (ChargeUse > 0)
			{
				E.Postfix.Append(" and powered");
			}
			int effectivePower = GetEffectivePower();
			E.Postfix.Append(", provides ").Append(effectivePower).Append(' ')
				.Append((effectivePower == 1) ? "unit" : "units")
				.Append(" of compute power to the local lattice.");
			AddStatusSummary(E.Postfix);
			E.Postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	public int GetEffectivePower()
	{
		int num = Power;
		int num2 = MyPowerLoadBonus(int.MinValue, 100, 10);
		if (num2 != 0)
		{
			num = num * (100 + num2) / 100;
		}
		return num;
	}
}
