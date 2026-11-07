using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AIGetMovementAbilityListEvent : IAIMoveCommandListEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetMovementAbilityListEvent), null, CountPool, ResetPool);

	private static List<AIGetMovementAbilityListEvent> Pool;

	private static int PoolCounter;

	public AIGetMovementAbilityListEvent()
	{
		base.ID = ID;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref AIGetMovementAbilityListEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AIGetMovementAbilityListEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public static List<AICommandList> GetFor(GameObject Actor, GameObject Target = null, Cell TargetCell = null, int Distance = -1, int StandoffDistance = 0)
	{
		AIGetMovementAbilityListEvent aIGetMovementAbilityListEvent = FromPool();
		if (TargetCell == null)
		{
			TargetCell = Target?.CurrentCell;
		}
		if (Distance == -1)
		{
			Distance = ((GameObject.Validate(ref Actor) && TargetCell != null) ? Actor.DistanceTo(TargetCell) : 0);
		}
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetMovementAbilityList"))
		{
			Event obj = Event.New("AIGetMovementAbilityList");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("TargetCell", TargetCell);
			obj.SetParameter("Distance", Distance);
			obj.SetParameter("StandoffDistance", StandoffDistance);
			obj.SetParameter("List", aIGetMovementAbilityListEvent.List);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetMovementMutationList"))
		{
			Event obj2 = Event.New("AIGetMovementMutationList");
			obj2.SetParameter("User", Actor);
			obj2.SetParameter("Target", Target);
			obj2.SetParameter("TargetCell", TargetCell);
			obj2.SetParameter("Distance", Distance);
			obj2.SetParameter("StandoffDistance", StandoffDistance);
			obj2.SetParameter("List", aIGetMovementAbilityListEvent.List);
			flag = Actor.FireEvent(obj2);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAICommandListEvent.CascadeLevel))
		{
			aIGetMovementAbilityListEvent.Actor = Actor;
			aIGetMovementAbilityListEvent.Target = Target;
			aIGetMovementAbilityListEvent.TargetCell = TargetCell;
			aIGetMovementAbilityListEvent.Distance = Distance;
			aIGetMovementAbilityListEvent.StandoffDistance = StandoffDistance;
			flag = Actor.HandleEvent(aIGetMovementAbilityListEvent);
		}
		return aIGetMovementAbilityListEvent.List;
	}
}
