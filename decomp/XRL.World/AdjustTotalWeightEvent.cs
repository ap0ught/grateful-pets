using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AdjustTotalWeightEvent : IWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AdjustTotalWeightEvent), null, CountPool, ResetPool);

	private static List<AdjustTotalWeightEvent> Pool;

	private static int PoolCounter;

	public AdjustTotalWeightEvent()
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

	public static void ResetTo(ref AdjustTotalWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AdjustTotalWeightEvent FromPool()
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

	public static AdjustTotalWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		AdjustTotalWeightEvent adjustTotalWeightEvent = FromPool();
		adjustTotalWeightEvent.Object = Object;
		adjustTotalWeightEvent.BaseWeight = BaseWeight;
		adjustTotalWeightEvent.Weight = Weight;
		return adjustTotalWeightEvent;
	}

	public static AdjustTotalWeightEvent FromPool(IWeightEvent PE)
	{
		AdjustTotalWeightEvent adjustTotalWeightEvent = FromPool();
		adjustTotalWeightEvent.Object = PE.Object;
		adjustTotalWeightEvent.BaseWeight = PE.BaseWeight;
		adjustTotalWeightEvent.Weight = PE.Weight;
		return adjustTotalWeightEvent;
	}

	public void AdjustWeight(double Factor)
	{
		Weight *= Factor;
	}
}
