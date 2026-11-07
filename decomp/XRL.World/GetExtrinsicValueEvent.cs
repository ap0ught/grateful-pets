using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetExtrinsicValueEvent : IValueEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetExtrinsicValueEvent), null, CountPool, ResetPool);

	private static List<GetExtrinsicValueEvent> Pool;

	private static int PoolCounter;

	public GetExtrinsicValueEvent()
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

	public static void ResetTo(ref GetExtrinsicValueEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetExtrinsicValueEvent FromPool()
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

	public static GetExtrinsicValueEvent FromPool(GameObject Object, double Value)
	{
		GetExtrinsicValueEvent getExtrinsicValueEvent = FromPool();
		getExtrinsicValueEvent.Object = Object;
		getExtrinsicValueEvent.Value = Value;
		return getExtrinsicValueEvent;
	}

	public static GetExtrinsicValueEvent FromPool(IValueEvent PE)
	{
		GetExtrinsicValueEvent getExtrinsicValueEvent = FromPool();
		getExtrinsicValueEvent.Object = PE.Object;
		getExtrinsicValueEvent.Value = PE.Value;
		return getExtrinsicValueEvent;
	}
}
