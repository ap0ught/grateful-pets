using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AIGetPassiveItemListEvent : IAIItemCommandListEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AIGetPassiveItemListEvent), null, CountPool, ResetPool);

	private static List<AIGetPassiveItemListEvent> Pool;

	private static int PoolCounter;

	public AIGetPassiveItemListEvent()
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

	public static void ResetTo(ref AIGetPassiveItemListEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AIGetPassiveItemListEvent FromPool()
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
		AIGetPassiveItemListEvent aIGetPassiveItemListEvent = FromPool();
		if (Target == null)
		{
			Target = Actor?.Target;
		}
		if (Distance == -1 && GameObject.Validate(ref Actor) && GameObject.Validate(ref Target))
		{
			Distance = Actor.DistanceTo(Target);
		}
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIGetPassiveItemList"))
		{
			Event obj = Event.New("AIGetPassiveItemList");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("Distance", Distance);
			obj.SetParameter("List", aIGetPassiveItemListEvent.List);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, IAIItemCommandListEvent.CascadeLevel))
		{
			aIGetPassiveItemListEvent.Actor = Actor;
			aIGetPassiveItemListEvent.Target = Target;
			aIGetPassiveItemListEvent.Distance = Distance;
			flag = Actor.HandleEvent(aIGetPassiveItemListEvent);
		}
		return aIGetPassiveItemListEvent.List;
	}
}
