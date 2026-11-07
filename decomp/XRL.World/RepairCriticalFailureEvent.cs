using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class RepairCriticalFailureEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(RepairCriticalFailureEvent), null, CountPool, ResetPool);

	private static List<RepairCriticalFailureEvent> Pool;

	private static int PoolCounter;

	public RepairCriticalFailureEvent()
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

	public static void ResetTo(ref RepairCriticalFailureEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static RepairCriticalFailureEvent FromPool()
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

	public static RepairCriticalFailureEvent FromPool(GameObject Actor, GameObject Item)
	{
		RepairCriticalFailureEvent repairCriticalFailureEvent = FromPool();
		repairCriticalFailureEvent.Actor = Actor;
		repairCriticalFailureEvent.Item = Item;
		return repairCriticalFailureEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		if (Actor.HasRegisteredEvent("RepairCriticalFailure") || Item.HasRegisteredEvent("RepairCriticalFailure"))
		{
			Event obj = Event.New("RepairCriticalFailure");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Item", Item);
			if (!Actor.FireEvent(obj) || !Item.FireEvent(obj))
			{
				return false;
			}
		}
		bool flag = Actor.WantEvent(ID, MinEvent.CascadeLevel);
		bool flag2 = Item.WantEvent(ID, MinEvent.CascadeLevel);
		if (flag | flag2)
		{
			RepairCriticalFailureEvent e = FromPool(Actor, Item);
			if ((flag && !Actor.HandleEvent(e)) || (flag2 && !Item.HandleEvent(e)))
			{
				return false;
			}
		}
		return true;
	}
}
