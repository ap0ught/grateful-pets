using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class PhaseOnSlam : IActivePart
{
	public PhaseOnSlam()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<SlamEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(SlamEvent E)
	{
		if (E.Target != null && IsObjectActivePartSubject(E.Actor))
		{
			if (E.Target.HasEffect<Phased>())
			{
				if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
				{
					E.Target.ApplyEffect(new Omniphase(E.SlamPower));
				}
			}
			else if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				E.Target.ApplyEffect(new Phased(E.SlamPower));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
