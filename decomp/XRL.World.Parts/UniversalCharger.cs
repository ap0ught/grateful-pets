using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class UniversalCharger : IPoweredPart
{
	public int ChargeRate = 10;

	public long ActiveTurn;

	public int UsedOnTurn;

	public UniversalCharger()
	{
		ChargeUse = 0;
		WorksOnInventory = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as UniversalCharger).ChargeRate != ChargeRate)
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
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && AnyActivePartSubjectWantsEvent(RechargeAvailableEvent.ID, IChargeEvent.CascadeLevel))
		{
			CheckTurn();
			int num = ((ChargeRate == 0 || E.Forced) ? (E.Amount - ChargeUse * E.Multiple) : Math.Min(ChargeRate * E.Multiple - UsedOnTurn, E.Amount - ChargeUse * E.Multiple));
			if (num > 0)
			{
				RechargeAvailableEvent rechargeAvailableEvent = RechargeAvailableEvent.FromPool(E);
				rechargeAvailableEvent.Amount = num;
				ActivePartSubjectsHandleEvent(rechargeAvailableEvent);
				if (rechargeAvailableEvent.Amount < num)
				{
					int num2 = num - rechargeAvailableEvent.Amount;
					if (E.Multiple > 1)
					{
						UsedOnTurn = num2 / E.Multiple;
					}
					else
					{
						UsedOnTurn += num2;
					}
					E.Amount -= num2 + ChargeUse;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && AnyActivePartSubjectWantsEvent(QueryRechargeStorageEvent.ID, IChargeEvent.CascadeLevel))
		{
			CheckTurn();
			int num = ChargeRate * E.Multiple - UsedOnTurn;
			if (num > 0)
			{
				QueryRechargeStorageEvent queryRechargeStorageEvent = QueryRechargeStorageEvent.FromPool(E);
				ActivePartSubjectsHandleEvent(queryRechargeStorageEvent);
				if (queryRechargeStorageEvent.Amount > E.Amount)
				{
					E.Amount += Math.Min(queryRechargeStorageEvent.Amount - E.Amount, num);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void CheckTurn()
	{
		if (XRLCore.Core != null && XRLCore.Core.Game != null && ActiveTurn < XRLCore.Core.Game.Turns)
		{
			UsedOnTurn = 0;
			ActiveTurn = XRLCore.Core.Game.Turns;
		}
	}
}
