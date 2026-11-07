using System;

namespace XRL.World.Parts;

[Serializable]
public class IntegralRecharger : IPoweredPart
{
	public int ChargeRate;

	public IntegralRecharger()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as IntegralRecharger).ChargeRate != ChargeRate)
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
		if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = E.Amount - ChargeUse * E.Multiple;
			if (ChargeRate != 0)
			{
				int num2 = ChargeRate * E.Multiple;
				if (num > num2)
				{
					num = num2;
				}
			}
			if (num > 0)
			{
				E.Amount -= RechargeAvailableEvent.Send(ParentObject, E, num);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = QueryRechargeStorageEvent.Retrieve(ParentObject, E);
			if (num > 0)
			{
				if (ChargeRate != 0)
				{
					E.Amount += Math.Min(ChargeRate, num);
				}
				else
				{
					E.Amount += num;
				}
			}
		}
		return base.HandleEvent(E);
	}
}
