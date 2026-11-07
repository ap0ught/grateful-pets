using System;
using UnityEngine;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, the gas multiplier is increased and dispersal
/// multiplier is decreased by a percentage equal to ((power load - 100) / 10),
/// i.e. 30% for the standard overload power load of 400.
/// </remarks>
[Serializable]
public class GasTumbler : IPoweredPart
{
	public int DispersalMultiplier = 25;

	public int DensityMultiplier = 200;

	public GasTumbler()
	{
		IsPowerLoadSensitive = true;
		WorksOnWearer = true;
		NameForStatus = "GasDensifier";
	}

	public override bool SameAs(IPart p)
	{
		return false;
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
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		int powerLoad = MyPowerLoadLevel();
		int num = GetDensity(powerLoad) - 100;
		int num2 = GetDispersal(powerLoad) - 100;
		E.Postfix.Append("\n{{rules|");
		if (num >= 0)
		{
			E.Postfix.Append("Gases you release are " + num + "% denser.");
		}
		else
		{
			E.Postfix.Append("Gases you release are " + -num + "% less dense.");
		}
		E.Postfix.Append("\n");
		if (num2 <= 0)
		{
			E.Postfix.Append("Gases you release disperse " + -num2 + "% slower.");
		}
		else
		{
			E.Postfix.Append("Gases you release disperse " + num2 + "% faster.");
		}
		E.Postfix.Append("}}");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "CreatorModifyGas");
		E.Actor.RegisterPartEvent(this, "CreatorModifyGasDispersal");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "CreatorModifyGas");
		E.Actor.UnregisterPartEvent(this, "CreatorModifyGasDispersal");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CreatorModifyGasDispersal")
		{
			int num = MyPowerLoadLevel();
			int? powerLoadLevel = num;
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
			{
				float num2 = (float)GetDispersal(num) / 100f;
				E.SetParameter("Rate", Mathf.RoundToInt((float)E.GetIntParameter("Rate") * num2));
			}
		}
		else if (E.ID == "CreatorModifyGas")
		{
			int num3 = MyPowerLoadLevel();
			int? powerLoadLevel = num3;
			if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel) && E.GetParameter("Gas") is Gas gas)
			{
				float num4 = (float)GetDensity(num3) / 100f;
				gas.Density = Mathf.RoundToInt((float)gas.Density * num4);
			}
		}
		return base.FireEvent(E);
	}

	public int GetDispersal(int PowerLoad)
	{
		int num = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num == 0)
		{
			return DispersalMultiplier;
		}
		return DispersalMultiplier * (100 - num) / 100;
	}

	public int GetDensity(int PowerLoad)
	{
		int num = MyPowerLoadBonus(PowerLoad, 100, 10);
		if (num == 0)
		{
			return DensityMultiplier;
		}
		return DensityMultiplier * (100 + num) / 100;
	}
}
