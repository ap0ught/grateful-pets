using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AIGetOffensiveItemListEvent : IAIItemCommandListEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetOffensiveItemListEvent), null, CountPool, ResetPool);

	private static List<AIGetOffensiveItemListEvent> Pool;

	private static int PoolCounter;

	public AIGetOffensiveItemListEvent()
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

	public static void ResetTo(ref AIGetOffensiveItemListEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AIGetOffensiveItemListEvent FromPool()
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

	public static List<AICommandList> GetFor(GameObject Actor, GameObject Target = null, int Distance = -1)
	{
		AIGetOffensiveItemListEvent aIGetOffensiveItemListEvent = FromPool();
		if (Target == null)
		{
			Target = Actor?.Target;
		}
		if (Distance == -1 && GameObject.Validate(ref Actor) && GameObject.Validate(ref Target))
		{
			Distance = Actor.DistanceTo(Target);
		}
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetOffensiveItemList"))
		{
			Event obj = Event.New("AIGetOffensiveItemList");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("Distance", Distance);
			obj.SetParameter("List", aIGetOffensiveItemListEvent.List);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAIItemCommandListEvent.CascadeLevel))
		{
			aIGetOffensiveItemListEvent.Actor = Actor;
			aIGetOffensiveItemListEvent.Target = Target;
			aIGetOffensiveItemListEvent.Distance = Distance;
			flag = Actor.HandleEvent(aIGetOffensiveItemListEvent);
		}
		return aIGetOffensiveItemListEvent.List;
	}
}
