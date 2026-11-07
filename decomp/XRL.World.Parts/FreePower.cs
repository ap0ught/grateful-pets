using System;

namespace XRL.World.Parts;

[Serializable]
public class FreePower : IPoweredPart
{
	public int ChargeFulfilled = 10;

	public bool ConsiderLive = true;

	public FreePower()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		WorksOnSelf = true;
	}

	public bool HasCharge(int Amount)
	{
		return ChargeFulfilled >= Amount;
	}

	public int GetCharge()
	{
		return ChargeFulfilled;
	}

	public override bool SameAs(IPart p)
	{
		FreePower freePower = p as FreePower;
		if (freePower.ChargeFulfilled != ChargeFulfilled)
		{
			return false;
		}
		if (freePower.ConsiderLive != ConsiderLive)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && (ID != PooledEvent<GetElectricalConductivityEvent>.ID || !ConsiderLive) && ID != QueryChargeEvent.ID && ID != TestChargeEvent.ID)
		{
			return ID == UseChargeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "ChargeFulfilled", ChargeFulfilled);
		E.AddEntry(this, "ConsiderLive", ConsiderLive);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == ParentObject && ConsiderLive)
		{
			E.MinValue(95);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = ChargeFulfilled - num;
			if (num2 > 0)
			{
				E.Amount += num2;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TestChargeEvent E)
	{
		if ((!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = Math.Min(E.Amount, ChargeFulfilled - num);
			if (num2 > 0)
			{
				E.Amount -= num2;
				if (E.Amount <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseChargeEvent E)
	{
		if (E.Pass == 1 && (!E.LiveOnly || ConsiderLive) && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ChargeUse * E.Multiple;
			int num2 = Math.Min(E.Amount, ChargeFulfilled - num);
			if (num2 > 0)
			{
				E.Amount -= num2;
				if (E.Amount <= 0)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}
}
