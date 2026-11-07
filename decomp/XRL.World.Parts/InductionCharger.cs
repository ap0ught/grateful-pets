using System;
using XRL.Core;

namespace XRL.World.Parts;

[Serializable]
public class InductionCharger : IPoweredPart
{
	public int ChargeRate = 10;

	public long ActiveTurn;

	public int UsedOnTurn;

	public InductionCharger()
	{
		ChargeUse = 0;
		WorksOnInventory = true;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as InductionCharger).ChargeRate != ChargeRate)
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
			int num = Math.Min(ChargeRate * E.Multiple - UsedOnTurn, E.Amount - ChargeUse * E.Multiple);
			if (num > 0 && AnyActivePartSubjectWantsEvent(InductionChargeEvent.ID, IChargeEvent.CascadeLevel))
			{
				InductionChargeEvent inductionChargeEvent = InductionChargeEvent.FromPool(E);
				inductionChargeEvent.Amount = num;
				ActivePartSubjectsHandleEvent(inductionChargeEvent);
				int num2 = num - inductionChargeEvent.Amount;
				if (num2 != 0)
				{
					E.Amount -= num2;
					if (num2 < 0)
					{
						num2 = -num2;
					}
					if (E.Multiple > 1)
					{
						num2 /= E.Multiple;
						UsedOnTurn = 0;
					}
					UsedOnTurn += num2;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && AnyActivePartSubjectWantsEvent(QueryInductionChargeStorageEvent.ID, IChargeEvent.CascadeLevel))
		{
			CheckTurn();
			int Available = ChargeRate - UsedOnTurn;
			if (Available > 0)
			{
				if (ActivePartHasSingleSubject())
				{
					QueryInductionChargeStorageEvent.Subprocess(GetActivePartFirstSubject(), E, ref Available);
				}
				else
				{
					foreach (GameObject activePartSubject in GetActivePartSubjects())
					{
						QueryInductionChargeStorageEvent.Subprocess(activePartSubject, E, ref Available);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public void CheckTurn()
	{
		if (The.Game != null && ActiveTurn < The.Game.Turns)
		{
			UsedOnTurn = 0;
			ActiveTurn = XRLCore.Core.Game.Turns;
		}
	}
}
