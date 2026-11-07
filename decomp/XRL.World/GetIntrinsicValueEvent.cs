using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetIntrinsicValueEvent : IValueEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetIntrinsicValueEvent), null, CountPool, ResetPool);

	private static List<GetIntrinsicValueEvent> Pool;

	private static int PoolCounter;

	public GetIntrinsicValueEvent()
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

	public static void ResetTo(ref GetIntrinsicValueEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetIntrinsicValueEvent FromPool()
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

	public static GetIntrinsicValueEvent FromPool(GameObject Object, double Value)
	{
		GetIntrinsicValueEvent getIntrinsicValueEvent = FromPool();
		getIntrinsicValueEvent.Object = Object;
		getIntrinsicValueEvent.Value = Value;
		return getIntrinsicValueEvent;
	}

	public static GetIntrinsicValueEvent FromPool(GameObject Object)
	{
		GetIntrinsicValueEvent getIntrinsicValueEvent = FromPool();
		getIntrinsicValueEvent.Object = Object;
		Commerce part = Object.GetPart<Commerce>();
		if (part != null)
		{
			getIntrinsicValueEvent.Value = part.Value;
		}
		return getIntrinsicValueEvent;
	}
}
