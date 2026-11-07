using System;

namespace XRL.World.Parts;

[Serializable]
public class Suspensor : IPoweredPart
{
	public int Force;

	public int PercentageForce;

	public Suspensor()
	{
		IsRealityDistortionBased = true;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		Suspensor suspensor = p as Suspensor;
		if (suspensor.Force != Force)
		{
			return false;
		}
		if (suspensor.PercentageForce != PercentageForce)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustTotalWeightEvent.ID && ID != BootSequenceDoneEvent.ID && ID != BootSequenceInitializedEvent.ID && ID != CellChangedEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID)
		{
			return ID == PowerSwitchFlippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AdjustTotalWeightEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (PercentageForce != 0)
			{
				E.Weight -= E.Weight * (double)PercentageForce / 100.0;
			}
			if (Force != 0)
			{
				E.Weight -= Force;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		FlushWeightCaches();
		ParentObject.Gravitate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		FlushWeightCaches();
		ParentObject.Gravitate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceDoneEvent E)
	{
		FlushWeightCaches();
		ParentObject.Gravitate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BootSequenceInitializedEvent E)
	{
		FlushWeightCaches();
		ParentObject.Gravitate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CellChangedEvent E)
	{
		FlushWeightCaches();
		ParentObject.Gravitate();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		FlushWeightCaches();
		ParentObject.Gravitate();
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		ConsumeChargeIfOperational(IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, IgnoreWorldMap: false, Amount);
	}
}
