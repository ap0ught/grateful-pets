using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class TryRemoveFromContextEvent : IRemoveFromContextEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(TryRemoveFromContextEvent), null, CountPool, ResetPool);

	private static List<TryRemoveFromContextEvent> Pool;

	private static int PoolCounter;

	public TryRemoveFromContextEvent()
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

	public static void ResetTo(ref TryRemoveFromContextEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static TryRemoveFromContextEvent FromPool()
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

	public static bool Check(GameObject Object, IEvent ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object))
		{
			TryRemoveFromContextEvent tryRemoveFromContextEvent = FromPool();
			tryRemoveFromContextEvent.Object = Object;
			flag = Object.HandleEvent(tryRemoveFromContextEvent);
			ParentEvent?.ProcessChildEvent(tryRemoveFromContextEvent);
		}
		return flag;
	}
}
