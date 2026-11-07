using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 32)]
public class AllowPolypPluckingEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AllowPolypPluckingEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 32;

	private static List<AllowPolypPluckingEvent> Pool;

	private static int PoolCounter;

	public Zone Zone;

	public GameObject Object;

	public GameObject Actor;

	public AllowPolypPluckingEvent()
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

	public static void ResetTo(ref AllowPolypPluckingEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AllowPolypPluckingEvent FromPool()
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
		Actor = null;
	}

	public static bool Check(Zone Zone, GameObject Object, GameObject Actor = null)
	{
		bool result = true;
		if (Zone != null && Zone.WantEvent(ID, CascadeLevel))
		{
			AllowPolypPluckingEvent allowPolypPluckingEvent = FromPool();
			allowPolypPluckingEvent.Zone = Zone;
			allowPolypPluckingEvent.Object = Object;
			allowPolypPluckingEvent.Actor = Actor;
			result = Zone.HandleEvent(allowPolypPluckingEvent);
		}
		return result;
	}
}
