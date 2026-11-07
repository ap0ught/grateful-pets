using System;

namespace XRL.World.Parts;

[Serializable]
public class InductionChargeReceiver : IPoweredPart
{
	public int ChargeRate = 10;

	public InductionChargeReceiver()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as InductionChargeReceiver).ChargeRate != ChargeRate)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != InductionChargeEvent.ID)
		{
			return ID == QueryInductionChargeStorageEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InductionChargeEvent E)
	{
		if (E.Amount > 0 && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = Math.Min(E.Amount, ChargeRate * E.Multiple);
			if (num > 0)
			{
				int num2 = ParentObject.ChargeAvailable(num, 0L);
				if (num2 > 0)
				{
					E.Amount -= num2;
					ConsumeCharge();
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryInductionChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.Amount += ParentObject.QueryChargeStorage(out var Transient, out var UnlimitedTransient);
			E.Transient += Transient;
			if (UnlimitedTransient)
			{
				E.UnlimitedTransient = true;
			}
		}
		return base.HandleEvent(E);
	}
}
