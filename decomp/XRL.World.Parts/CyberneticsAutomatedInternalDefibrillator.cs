using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsAutomatedInternalDefibrillator : IPoweredPart
{
	public int Chance = 50;

	public CyberneticsAutomatedInternalDefibrillator()
	{
		ChargeUse = 0;
		NameForStatus = "AutomatedInternalDefibrillator";
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules("Compute power on the local lattice increases this item's effectiveness.");
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		GameObject implantee = ParentObject.Implantee;
		if (implantee != null && implantee.HasEffect<CardiacArrest>() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && GetChance(implantee).in100())
		{
			implantee.RemoveEffect<CardiacArrest>();
		}
	}

	public int GetChance(GameObject who = null)
	{
		int num = Chance;
		if (who == null)
		{
			who = ParentObject.Implantee;
		}
		if (who != null)
		{
			num = GetAvailableComputePowerEvent.AdjustUp(who, num);
		}
		return num;
	}
}
