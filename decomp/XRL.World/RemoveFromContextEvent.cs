using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class RemoveFromContextEvent : IRemoveFromContextEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(RemoveFromContextEvent), null, CountPool, ResetPool);

	private static List<RemoveFromContextEvent> Pool;

	private static int PoolCounter;

	public RemoveFromContextEvent()
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

	public static void ResetTo(ref RemoveFromContextEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static RemoveFromContextEvent FromPool()
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

	public static void Send(GameObject Object, IEvent ParentEvent = null)
	{
		if (GameObject.Validate(ref Object))
		{
			RemoveFromContextEvent E = FromPool();
			E.Object = Object;
			Object.HandleEvent(E);
			ParentEvent?.ProcessChildEvent(E);
			ResetTo(ref E);
		}
	}
}
