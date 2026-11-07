using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetExtrinsicWeightEvent : IWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetExtrinsicWeightEvent), null, CountPool, ResetPool);

	private static List<GetExtrinsicWeightEvent> Pool;

	private static int PoolCounter;

	public GetExtrinsicWeightEvent()
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

	public static void ResetTo(ref GetExtrinsicWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetExtrinsicWeightEvent FromPool()
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

	public static GetExtrinsicWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		GetExtrinsicWeightEvent getExtrinsicWeightEvent = FromPool();
		getExtrinsicWeightEvent.Object = Object;
		getExtrinsicWeightEvent.BaseWeight = BaseWeight;
		getExtrinsicWeightEvent.Weight = Weight;
		return getExtrinsicWeightEvent;
	}

	public static GetExtrinsicWeightEvent FromPool(IWeightEvent PE)
	{
		GetExtrinsicWeightEvent getExtrinsicWeightEvent = FromPool();
		getExtrinsicWeightEvent.Object = PE.Object;
		getExtrinsicWeightEvent.BaseWeight = PE.BaseWeight;
		getExtrinsicWeightEvent.Weight = PE.Weight;
		return getExtrinsicWeightEvent;
	}
}
