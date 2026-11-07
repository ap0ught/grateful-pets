using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetIntrinsicWeightEvent : IWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetIntrinsicWeightEvent), null, CountPool, ResetPool);

	private static List<GetIntrinsicWeightEvent> Pool;

	private static int PoolCounter;

	public GetIntrinsicWeightEvent()
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

	public static void ResetTo(ref GetIntrinsicWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetIntrinsicWeightEvent FromPool()
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

	public static GetIntrinsicWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		GetIntrinsicWeightEvent getIntrinsicWeightEvent = FromPool();
		getIntrinsicWeightEvent.Object = Object;
		getIntrinsicWeightEvent.BaseWeight = BaseWeight;
		getIntrinsicWeightEvent.Weight = Weight;
		return getIntrinsicWeightEvent;
	}

	public static GetIntrinsicWeightEvent FromPool(GameObject Object, double Weight)
	{
		GetIntrinsicWeightEvent getIntrinsicWeightEvent = FromPool();
		getIntrinsicWeightEvent.Object = Object;
		getIntrinsicWeightEvent.BaseWeight = Weight;
		getIntrinsicWeightEvent.Weight = Weight;
		return getIntrinsicWeightEvent;
	}

	public static GetIntrinsicWeightEvent FromPool(GameObject Object, int Weight)
	{
		return FromPool(Object, (double)Weight);
	}
}
