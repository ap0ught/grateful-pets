using System;
using System.Linq;

namespace XRL.World.Parts;

[Serializable]
public class AilingQuickness : IActivePart
{
	public int SpeedBonus = 5;

	[NonSerialized]
	private int LastCount = -1;

	public AilingQuickness()
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
		if (SpeedBonus > 0)
		{
			E.Postfix.Compound("{{rules|", '\n');
			Statistic.AppendStatAdjustDescription(E.Postfix, "Speed", SpeedBonus);
			E.Postfix.Append(" per negative status effect}}");
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
			int num = Enumerable.Count(activePartFirstSubject.Effects, (Effect x) => x.IsOfType(33554432));
			if (LastCount != num)
			{
				LastCount = num;
				if (num <= 0)
				{
					StatShifter.RemoveStatShifts(activePartFirstSubject);
				}
				else
				{
					StatShifter.SetStatShift(activePartFirstSubject, "Speed", SpeedBonus * num);
				}
			}
		}
		else
		{
			StatShifter.RemoveStatShifts();
			LastCount = -1;
		}
	}
}
