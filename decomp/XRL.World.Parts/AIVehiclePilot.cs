using System;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class AIVehiclePilot : AIBehaviorPart
{
	public string Blueprint;

	public string Tag;

	public string Type;

	public int SearchRadius = 15;

	public bool OnCombatStart = true;

	[NonSerialized]
	private DelegateGoal Goal;

	[NonSerialized]
	private GameObject Vehicle;

	[NonSerialized]
	private Interior Interior;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!ParentObject.IsBusy() && ParentObject.Target == null && !ParentObject.IsPlayer())
		{
			TryPilotVehicle();
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		if (OnCombatStart)
		{
			Registrar.Register("AICreateKill");
		}
	}

	public GameObject FindVehicle(bool Search = false)
	{
		if (ParentObject.CurrentZone is InteriorZone interiorZone && IsValidVehicle(interiorZone.ParentObject))
		{
			return interiorZone.ParentObject;
		}
		if (Search)
		{
			foreach (Cell item in ParentObject.CurrentCell.YieldAdjacentCells(SearchRadius, LocalOnly: true))
			{
				foreach (GameObject @object in item.Objects)
				{
					if (IsValidVehicle(@object))
					{
						return @object;
					}
				}
			}
		}
		return null;
	}

	public bool TryPilotVehicle(bool Search = false)
	{
		Vehicle = FindVehicle(Search);
		Interior = Vehicle?.GetPart<Interior>();
		if (Vehicle != null && Interior != null)
		{
			if (Goal == null)
			{
				Goal = new DelegateGoal(TakeAction);
			}
			Goal.SetCanFight = false;
			ParentObject.Brain.Goals.Clear();
			ParentObject.Brain.PushGoal(Goal);
			return true;
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (OnCombatStart && E.ID == "AICreateKill" && !ParentObject.AreHostilesAdjacent() && TryPilotVehicle(Search: true))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public bool IsValidVehicle(GameObject Object)
	{
		Vehicle vehicle = Object?.GetPart<Vehicle>();
		if (vehicle == null || vehicle.PilotID != null)
		{
			return false;
		}
		if (!vehicle.CanBePilotedBy(ParentObject))
		{
			return false;
		}
		if (!Blueprint.IsNullOrEmpty() && Object.Blueprint == Blueprint)
		{
			return true;
		}
		if (!Tag.IsNullOrEmpty() && Object.HasTagOrProperty(Tag))
		{
			return true;
		}
		if (!Type.IsNullOrEmpty() && vehicle.Type == Type)
		{
			return true;
		}
		return false;
	}

	public bool IsValidSeat(GameObject Object)
	{
		if (Object.HasPart(typeof(VehicleSeat)))
		{
			return Object.CurrentCell.IsPassable(ParentObject);
		}
		return false;
	}

	public void TakeAction(GoalHandler Handler)
	{
		Zone currentZone = ParentObject.CurrentZone;
		if (ParentObject.IsPlayer())
		{
			Handler.Pop();
			return;
		}
		if (currentZone.ZoneID != Interior.ZoneID)
		{
			Handler.PushChildGoal(new MoveToInterior(Vehicle));
			return;
		}
		if (ParentObject.TryGetEffect<Sitting>(out var Effect))
		{
			if (!GameObject.Validate(Effect.SittingOn) || Effect.SittingOn.DistanceTo(ParentObject) >= 1 || !ParentObject.HasEffect(typeof(Piloting)))
			{
				ParentObject.RemoveEffect(Effect);
			}
			return;
		}
		currentZone.MarkActive();
		currentZone.Activated();
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
			Handler.PushChildGoal(new MoveToExterior());
		}
	}
}
