using System;

namespace XRL.World.Parts;

[Serializable]
public class MoveCostMultiplier : IPart
{
	public int Amount = -5;

	public MoveCostMultiplier()
	{
	}

	public MoveCostMultiplier(int Amount)
		: this()
	{
		this.Amount = Amount;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as MoveCostMultiplier).Amount != Amount)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		GameObject actor = E.Actor;
		StatShifter.SetStatShift(actor, "MoveSpeed", Amount);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		StatShifter.RemoveStatShifts(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Amount != 0)
		{
			E.Postfix.Append("\n{{rules|").Append((-Amount).Signed()).Append(" move speed}}");
		}
		return base.HandleEvent(E);
	}
}
