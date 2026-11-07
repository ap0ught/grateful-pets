using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AIGetRetreatAbilityListEvent : IAIMoveCommandListEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetRetreatAbilityListEvent), null, CountPool, ResetPool);

	private static List<AIGetRetreatAbilityListEvent> Pool;

	private static int PoolCounter;

	public Cell AvoidCell;

	public AIGetRetreatAbilityListEvent()
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

	public static void ResetTo(ref AIGetRetreatAbilityListEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AIGetRetreatAbilityListEvent FromPool()
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

	public override void Reset()
	{
		base.Reset();
		AvoidCell = null;
	}

	public static List<AICommandList> GetFor(GameObject Actor, GameObject Target = null, Cell TargetCell = null, Cell AvoidCell = null, int Distance = -1, int StandoffDistance = 0)
	{
		AIGetRetreatAbilityListEvent aIGetRetreatAbilityListEvent = FromPool();
		if (Distance == -1)
		{
			Distance = ((GameObject.Validate(ref Actor) && TargetCell != null) ? Actor.DistanceTo(TargetCell) : 0);
		}
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetRetreatAbilityList"))
		{
			Event obj = Event.New("AIGetRetreatAbilityList");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("TargetCell", TargetCell);
			obj.SetParameter("AvoidCell", AvoidCell);
			obj.SetParameter("Distance", Distance);
			obj.SetParameter("StandoffDistance", StandoffDistance);
			obj.SetParameter("List", aIGetRetreatAbilityListEvent.List);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAICommandListEvent.CascadeLevel))
		{
			aIGetRetreatAbilityListEvent.Actor = Actor;
			aIGetRetreatAbilityListEvent.Target = Target;
			aIGetRetreatAbilityListEvent.TargetCell = TargetCell;
			aIGetRetreatAbilityListEvent.AvoidCell = AvoidCell;
			aIGetRetreatAbilityListEvent.Distance = Distance;
			aIGetRetreatAbilityListEvent.StandoffDistance = StandoffDistance;
			flag = Actor.HandleEvent(aIGetRetreatAbilityListEvent);
		}
		return aIGetRetreatAbilityListEvent.List;
	}
}
