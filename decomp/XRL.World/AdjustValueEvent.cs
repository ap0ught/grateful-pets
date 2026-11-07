using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AdjustValueEvent : IValueEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AdjustValueEvent), null, CountPool, ResetPool);

	private static List<AdjustValueEvent> Pool;

	private static int PoolCounter;

	public AdjustValueEvent()
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

	public static void ResetTo(ref AdjustValueEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AdjustValueEvent FromPool()
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

	public static AdjustValueEvent FromPool(GameObject Object, double Value)
	{
		AdjustValueEvent adjustValueEvent = FromPool();
		adjustValueEvent.Object = Object;
		adjustValueEvent.Value = Value;
		return adjustValueEvent;
	}

	public static AdjustValueEvent FromPool(IValueEvent PE)
	{
		AdjustValueEvent adjustValueEvent = FromPool();
		adjustValueEvent.Object = PE.Object;
		adjustValueEvent.Value = PE.Value;
		return adjustValueEvent;
	}

	public void AdjustValue(double Factor)
	{
		Value *= Factor;
	}
}
