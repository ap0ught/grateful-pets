using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class AfterZoneBuiltEvent : IZoneEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterZoneBuiltEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 15;

	private static List<AfterZoneBuiltEvent> Pool;

	private static int PoolCounter;

	public AfterZoneBuiltEvent()
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

	public static void ResetTo(ref AfterZoneBuiltEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterZoneBuiltEvent FromPool()
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
		AfterZoneBuiltEvent E = FromPool();
		E.Zone = Zone;
		The.Game.HandleEvent(E);
		Zone.HandleEvent(E);
		ResetTo(ref E);
	}
}
