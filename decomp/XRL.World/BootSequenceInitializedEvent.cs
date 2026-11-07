using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BootSequenceInitializedEvent : IBootSequenceEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BootSequenceInitializedEvent), null, CountPool, ResetPool);

	private static List<BootSequenceInitializedEvent> Pool;

	private static int PoolCounter;

	public BootSequenceInitializedEvent()
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

	public static void ResetTo(ref BootSequenceInitializedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BootSequenceInitializedEvent FromPool()
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

	public static BootSequenceInitializedEvent FromPool(GameObject Object)
	{
		BootSequenceInitializedEvent bootSequenceInitializedEvent = FromPool();
		bootSequenceInitializedEvent.Object = Object;
		return bootSequenceInitializedEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.FireEvent("BootSequenceInitialized") && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}
