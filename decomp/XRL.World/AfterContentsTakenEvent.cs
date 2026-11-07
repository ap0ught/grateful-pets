using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterContentsTakenEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterContentsTakenEvent), null, CountPool, ResetPool);

	private static List<AfterContentsTakenEvent> Pool;

	private static int PoolCounter;

	public GameObject Container;

	public AfterContentsTakenEvent()
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

	public static void ResetTo(ref AfterContentsTakenEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterContentsTakenEvent FromPool()
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
		Container = null;
	}

	public static void Send(GameObject Actor, GameObject Container, GameObject Item)
	{
		AfterContentsTakenEvent E = FromPool();
		E.Actor = Actor;
		E.Container = Container;
		E.Item = Item;
		Container.HandleEvent(E);
		ResetTo(ref E);
	}
}
