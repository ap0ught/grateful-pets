using System;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AIPassenger : AIBehaviorPart
{
	public GameObjectReference Vehicle;

	public bool Removable = true;

	public bool End;

	[NonSerialized]
	public Interior _Interior;

	[NonSerialized]
	private DelegateGoal Goal;

	public Interior Interior => _Interior ?? (_Interior = Vehicle.Object?.GetPart<Interior>());

	public AIPassenger()
	{
	}

	public AIPassenger(GameObject Vehicle, bool Removable = true)
	{
		this.Vehicle = Vehicle.Reference();
		this.Removable = Removable;
	}

	public bool CheckPassengerSeat()
	{
		if (ParentObject.IsBusy())
		{
			return true;
		}
		if (ParentObject.IsPlayer() || (ParentObject.PartyLeader != null && !ParentObject.Brain.Staying))
		{
			return false;
		}
		if (!ParentObject.FireEvent("CanAIDoIndependentBehavior"))
		{
			return false;
		}
		if (!GameObject.Validate(Interior?.ParentObject))
		{
			_Interior = null;
			return false;
		}
		if (Goal == null)
		{
			Goal = new DelegateGoal(TakeAction);
		}
		if (!ParentObject.Brain.Goals.Contains(Goal))
		{
			Goal.SetCanFight = false;
			Goal.SetNonAggressive = true;
			ParentObject.Brain.PushGoal(Goal);
		}
		return true;
	}

	public bool IsValidSeat(GameObject Object)
	{
		if (Object.HasPart(typeof(Chair)))
		{
			return Object.CurrentCell.IsPassable(ParentObject);
		}
		return false;
	}

	public void TakeAction(GoalHandler Handler)
	{
		if (!End && !GameObject.Validate(Interior?.ParentObject))
		{
			End = true;
		}
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone.ZoneID != Interior.ZoneID)
		{
			if (End)
			{
				ParentObject.Brain.Staying = false;
				Handler.Pop();
				if (Removable)
				{
					ParentObject.RemovePart(this);
				}
			}
			else
			{
				Handler.PushChildGoal(new MoveToInterior(Vehicle.Object));
			}
			return;
		}
		Goal.SetCanFight = null;
		Goal.SetNonAggressive = null;
		if (End)
		{
			Handler.PushChildGoal(new MoveToExterior());
			return;
		}
		if (ParentObject.TryGetEffect<Sitting>(out var Effect))
		{
			if (!GameObject.Validate(Effect.SittingOn) || Effect.SittingOn.DistanceTo(ParentObject) >= 1)
			{
				ParentObject.RemoveEffect(Effect);
			}
			else
			{
				ParentObject.UseEnergy(1000);
			}
			return;
		}
		GameObject gameObject = currentZone.FindClosestObject(ParentObject, IsValidSeat, ExploredOnly: false, VisibleOnly: true, IncludeSelf: false);
		if (gameObject != null)
		{
			if (ParentObject.DistanceTo(gameObject) <= 1)
			{
				gameObject.GetPart<Chair>().SitDown(ParentObject);
			}
			else
			{
				Handler.PushChildGoal(new MoveTo(gameObject, careful: false, overridesCombat: false, 1));
			}
		}
		else
		{
			End = true;
			Handler.PushChildGoal(new MoveToExterior());
		}
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (ParentObject == null)
		{
			return;
		}
		if (ParentObject.IsPlayer() || (ParentObject.PartyLeader != null && !ParentObject.Brain.Staying))
		{
			End = true;
		}
		if (!CheckPassengerSeat() && Removable)
		{
			GameObject Object = ParentObject.Brain.PartyLeader;
			if (GameObject.Validate(ref Object) && ParentObject.Brain.Staying && ParentObject.InSameZone(Object))
			{
				ParentObject.Brain.Staying = false;
			}
			ParentObject.RemovePart(this);
		}
	}
}
