using System;

namespace XRL.World.Parts;

[Serializable]
public class ChargeSink : IPoweredPart
{
	public ChargeSink()
	{
		WorksOnSelf = true;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
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
