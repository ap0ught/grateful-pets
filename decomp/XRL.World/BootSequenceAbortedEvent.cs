using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BootSequenceAbortedEvent : IBootSequenceEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BootSequenceAbortedEvent), null, CountPool, ResetPool);

	private static List<BootSequenceAbortedEvent> Pool;

	private static int PoolCounter;

	public BootSequenceAbortedEvent()
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

	public static void ResetTo(ref BootSequenceAbortedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BootSequenceAbortedEvent FromPool()
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

	public static BootSequenceAbortedEvent FromPool(GameObject Object)
	{
		BootSequenceAbortedEvent bootSequenceAbortedEvent = FromPool();
		bootSequenceAbortedEvent.Object = Object;
		return bootSequenceAbortedEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.FireEvent("BootSequenceAborted") && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}
