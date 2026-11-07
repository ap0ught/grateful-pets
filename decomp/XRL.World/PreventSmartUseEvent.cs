using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class PreventSmartUseEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(PreventSmartUseEvent), null, CountPool, ResetPool);

	private static List<PreventSmartUseEvent> Pool;

	private static int PoolCounter;

	public PreventSmartUseEvent()
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

	public static void ResetTo(ref PreventSmartUseEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static PreventSmartUseEvent FromPool()
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

	public static PreventSmartUseEvent FromPool(GameObject Actor, GameObject Item)
	{
		PreventSmartUseEvent preventSmartUseEvent = FromPool();
		preventSmartUseEvent.Actor = Actor;
		preventSmartUseEvent.Item = Item;
		return preventSmartUseEvent;
	}
}
