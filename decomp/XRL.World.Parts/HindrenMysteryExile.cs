using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

internal class HindrenMysteryExile : IPart
{
	public GlobalLocation Destination;

	private Zone beyLah => KithAndKinGameState.Instance.getBeyLahZone();

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == SuspendingEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(SuspendingEvent E)
	{
		if (!ParentObject.IsPlayerControlled())
		{
			if (Destination == null)
			{
				ParentObject.Destroy();
			}
			else if (ParentObject.CurrentZone.ZoneID != Destination.ZoneID)
			{
				ParentObject.DirectMoveTo(Destination, 0, Forced: true, IgnoreCombat: true);
			}
		}
		return base.HandleEvent(E);
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
		CheckExile();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneFreezing" && !ParentObject.IsPlayerControlled())
		{
			if (Destination == null)
			{
				ParentObject.Destroy();
			}
			else if (ParentObject.CurrentZone.ZoneID != Destination.ZoneID)
			{
				ParentObject.DirectMoveTo(Destination, 0, Forced: true, IgnoreCombat: true);
			}
		}
		return base.FireEvent(E);
	}

	public bool CheckExile()
	{
		if (ParentObject.IsBusy())
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		if (ParentObject.IsPlayerControlled() || ParentObject.GetIntProperty("TurnsAsPlayerMinion") > 0)
		{
			ParentObject.RemovePart(this);
			return false;
		}
		if (ParentObject.CurrentZone.ZoneID == beyLah.ZoneID || ParentObject.CurrentZone.IsActive())
		{
			if (Destination == null)
			{
				ParentObject.Brain.PushGoal(new MoveToZone(ParentObject.CurrentZone.GetZoneIDFromDirection("W")));
				ParentObject.Brain.StartingCell = null;
			}
			else
			{
				ParentObject.Brain.PushGoal(new MoveToGlobal(Destination));
				ParentObject.Brain.StartingCell = Destination;
			}
		}
		else if (Destination == null)
		{
			ParentObject.Destroy();
		}
		else if (ParentObject.CurrentZone.ZoneID != Destination.ZoneID)
		{
			ParentObject.Brain.PushGoal(new MoveToGlobal(Destination));
		}
		return true;
	}
}
