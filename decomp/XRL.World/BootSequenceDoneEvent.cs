using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BootSequenceDoneEvent : IBootSequenceEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BootSequenceDoneEvent), null, CountPool, ResetPool);

	private static List<BootSequenceDoneEvent> Pool;

	private static int PoolCounter;

	public BootSequenceDoneEvent()
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

	public static void ResetTo(ref BootSequenceDoneEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BootSequenceDoneEvent FromPool()
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

	public static BootSequenceDoneEvent FromPool(GameObject Object)
	{
		BootSequenceDoneEvent bootSequenceDoneEvent = FromPool();
		bootSequenceDoneEvent.Object = Object;
		return bootSequenceDoneEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.FireEvent("BootSequenceDone") && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}
