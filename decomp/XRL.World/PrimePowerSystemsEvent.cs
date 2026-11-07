using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class PrimePowerSystemsEvent : IZoneEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(PrimePowerSystemsEvent), null, CountPool, ResetPool);

	private static List<PrimePowerSystemsEvent> Pool;

	private static int PoolCounter;

	public PrimePowerSystemsEvent()
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

	public static void ResetTo(ref PrimePowerSystemsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static PrimePowerSystemsEvent FromPool()
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

	public static void Send(Zone Zone)
	{
		if (Zone != null && Zone.WantEvent(ID, MinEvent.CascadeLevel))
		{
			PrimePowerSystemsEvent primePowerSystemsEvent = FromPool();
			primePowerSystemsEvent.Zone = Zone;
			Zone.HandleEvent(primePowerSystemsEvent);
		}
	}
}
