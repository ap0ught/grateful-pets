using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 15)]
public class PowerUpdatedEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(PowerUpdatedEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 15;

	private static List<PowerUpdatedEvent> Pool;

	private static int PoolCounter;

	public Zone Zone;

	public GameObject Object;

	public PowerUpdatedEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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

	public static void ResetTo(ref PowerUpdatedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static PowerUpdatedEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Zone = null;
		Object = null;
	}

	public static void Send(Zone Zone, GameObject Object = null)
	{
		if (Zone != null && Zone.WantEvent(ID, CascadeLevel))
		{
			PowerUpdatedEvent powerUpdatedEvent = FromPool();
			powerUpdatedEvent.Zone = Zone;
			powerUpdatedEvent.Object = Object;
			Zone.HandleEvent(powerUpdatedEvent);
		}
	}
}
