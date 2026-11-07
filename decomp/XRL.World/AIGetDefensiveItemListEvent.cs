using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AIGetDefensiveItemListEvent : IAIItemCommandListEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetDefensiveItemListEvent), null, CountPool, ResetPool);

	private static List<AIGetDefensiveItemListEvent> Pool;

	private static int PoolCounter;

	public AIGetDefensiveItemListEvent()
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

	public static void ResetTo(ref AIGetDefensiveItemListEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AIGetDefensiveItemListEvent FromPool()
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
		AIGetDefensiveItemListEvent aIGetDefensiveItemListEvent = FromPool();
		if (Target == null)
		{
			Target = Actor?.Target;
		}
		if (Distance == -1 && GameObject.Validate(ref Actor) && GameObject.Validate(ref Target))
		{
			Distance = Actor.DistanceTo(Target);
		}
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetDefensiveItemList"))
		{
			Event obj = Event.New("AIGetDefensiveItemList");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("Distance", Distance);
			obj.SetParameter("List", aIGetDefensiveItemListEvent.List);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAIItemCommandListEvent.CascadeLevel))
		{
			aIGetDefensiveItemListEvent.Actor = Actor;
			aIGetDefensiveItemListEvent.Target = Target;
			aIGetDefensiveItemListEvent.Distance = Distance;
			flag = Actor.HandleEvent(aIGetDefensiveItemListEvent);
		}
		return aIGetDefensiveItemListEvent.List;
	}
}
