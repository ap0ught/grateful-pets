using System;

namespace XRL.World.Parts;

[Serializable]
public class FungalFortitude : IActivePart
{
	public int AVBonus = 1;

	public int ResistBonus = 4;

	[NonSerialized]
	private int LastCount = -1;

	public FungalFortitude()
	{
		WorksOnHolder = true;
		WorksOnCarrier = true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CalculateBonus(UseCharge: true, Amount);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != TakenEvent.ID)
		{
			return ID == DroppedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (AVBonus > 0)
		{
			E.Postfix.Compound("{{rules|", '\n').AppendSigned(AVBonus).Append(" AV per fungal infection");
		}
		if (ResistBonus > 0)
		{
			E.Postfix.Compound("{{rules|", '\n').AppendSigned(ResistBonus).Append(" to all resists per fungal infection");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		CalculateBonus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		CalculateBonus();
		return base.HandleEvent(E);
	}

	public void CalculateBonus(bool UseCharge = false, int MultipleCharge = 1)
	{
		if (IsReady(UseCharge, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, MultipleCharge, null, UseChargeIfUnpowered: false, 0L))
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			int count = 0;
			activePartFirstSubject.ForeachEquippedObject((GameObject Object) =>
			{
				if (Object.HasTag("FungalInfection"))
				{
					count++;
				}
			});
			if (LastCount != count)
			{
				LastCount = count;
				if (count <= 0)
				{
					StatShifter.RemoveStatShifts(activePartFirstSubject);
					return;
				}
				StatShifter.SetStatShift(activePartFirstSubject, "AV", AVBonus * count);
				StatShifter.SetStatShift(activePartFirstSubject, "HeatResistance", ResistBonus * count);
				StatShifter.SetStatShift(activePartFirstSubject, "ColdResistance", ResistBonus * count);
				StatShifter.SetStatShift(activePartFirstSubject, "ElectricResistance", ResistBonus * count);
				StatShifter.SetStatShift(activePartFirstSubject, "AcidResistance", ResistBonus * count);
			}
		}
		else
		{
			StatShifter.RemoveStatShifts();
			LastCount = -1;
		}
	}
}
