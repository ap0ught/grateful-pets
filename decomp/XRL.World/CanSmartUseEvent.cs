using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanSmartUseEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanSmartUseEvent), null, CountPool, ResetPool);

	private static List<CanSmartUseEvent> Pool;

	private static int PoolCounter;

	public int MinPriority;

	public CanSmartUseEvent()
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

	public static void ResetTo(ref CanSmartUseEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CanSmartUseEvent FromPool()
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
		MinPriority = 0;
	}

	public static CanSmartUseEvent FromPool(GameObject Actor, GameObject Item, int MinPriority)
	{
		CanSmartUseEvent canSmartUseEvent = FromPool();
		canSmartUseEvent.Actor = Actor;
		canSmartUseEvent.Item = Item;
		canSmartUseEvent.MinPriority = MinPriority;
		return canSmartUseEvent;
	}
}
