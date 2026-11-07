using System;

namespace XRL.World.Parts;

[Serializable]
public class Cursed : IActivePart
{
	public bool RevealInDescription;

	public string DescriptionPostfix = "Cannot be removed once equipped.";

	public Cursed()
	{
		WorksOnEquipper = true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeginBeingUnequippedEvent>.ID && ID != PooledEvent<CanBeUnequippedEvent>.ID && (ID != GetShortDescriptionEvent.ID || !RevealInDescription) && ID != PooledEvent<GetSlotsRequiredEvent>.ID)
		{
			return ID == PooledEvent<IsAfflictionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeUnequippedEvent E)
	{
		if (!E.Forced && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginBeingUnequippedEvent E)
	{
		if (!E.Forced && ParentObject.Equipped != null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			E.AddFailureMessage("You can't remove " + ParentObject.t() + ".");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetSlotsRequiredEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.AllowReduction = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (RevealInDescription)
		{
			E.Postfix.AppendRules(DescriptionPostfix);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsAfflictionEvent E)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
