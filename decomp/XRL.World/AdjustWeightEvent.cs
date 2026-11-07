using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AdjustWeightEvent : IWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AdjustWeightEvent), null, CountPool, ResetPool);

	private static List<AdjustWeightEvent> Pool;

	private static int PoolCounter;

	public AdjustWeightEvent()
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

	public static void ResetTo(ref AdjustWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AdjustWeightEvent FromPool()
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

	public static AdjustWeightEvent FromPool(GameObject Object, double BaseWeight, double Weight)
	{
		AdjustWeightEvent adjustWeightEvent = FromPool();
		adjustWeightEvent.Object = Object;
		adjustWeightEvent.BaseWeight = BaseWeight;
		adjustWeightEvent.Weight = Weight;
		return adjustWeightEvent;
	}

	public static AdjustWeightEvent FromPool(IWeightEvent PE)
	{
		AdjustWeightEvent adjustWeightEvent = FromPool();
		adjustWeightEvent.Object = PE.Object;
		adjustWeightEvent.BaseWeight = PE.BaseWeight;
		adjustWeightEvent.Weight = PE.Weight;
		return adjustWeightEvent;
	}

	public void AdjustWeight(double Factor)
	{
		Weight *= Factor;
	}
}
