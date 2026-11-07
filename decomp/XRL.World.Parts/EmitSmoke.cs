using System;

namespace XRL.World.Parts;

[Serializable]
public class EmitSmoke : IActivePart
{
	public int ChanceOnEndTurn;

	public int ChanceOnEquipperEnteredCell;

	public string StartAngle = "85";

	public string EndAngle = "185";

	public EmitSmoke()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		EmitSmoke emitSmoke = p as EmitSmoke;
		if (emitSmoke.ChanceOnEndTurn != ChanceOnEndTurn)
		{
			return false;
		}
		if (emitSmoke.ChanceOnEquipperEnteredCell != ChanceOnEquipperEnteredCell)
		{
			return false;
		}
		if (emitSmoke.StartAngle != StartAngle)
		{
			return false;
		}
		if (emitSmoke.EndAngle != EndAngle)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != SingletonEvent<EndTurnEvent>.ID || ChanceOnEndTurn <= 0) && (ID != EquippedEvent.ID || ChanceOnEquipperEnteredCell <= 0))
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (ChanceOnEndTurn.in100())
		{
			Smoke();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" && ChanceOnEquipperEnteredCell.in100())
		{
			ParentObject.Smoke();
		}
		return base.FireEvent(E);
	}

	public bool Smoke()
	{
		if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		ParentObject.Smoke(StartAngle.RollCached(), EndAngle.RollCached());
		return true;
	}
}
