using System;

namespace XRL.World.Parts;

[Serializable]
public class AttributeMultiplier : IPart
{
	public string Attribute;

	public int Percent;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		float num = (float)ParentObject.GetStat(Attribute).BaseValue * ((float)Percent / 100f);
		StatShifter.SetStatShift(Attribute, (int)num);
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
		int value = (Statistic.IsInverseBenefit(Attribute) ? (-Percent) : Percent);
		E.Postfix.AppendSigned(value).Append("% ").Append(Statistic.GetStatDisplayName(Attribute));
		return base.HandleEvent(E);
	}
}
