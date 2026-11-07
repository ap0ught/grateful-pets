using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Pool)]
public class AllowLiquidCollectionEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AllowLiquidCollectionEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 0;

	private static List<AllowLiquidCollectionEvent> Pool;

	private static int PoolCounter;

	public AllowLiquidCollectionEvent()
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

	public static void ResetTo(ref AllowLiquidCollectionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AllowLiquidCollectionEvent FromPool()
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

	public static AllowLiquidCollectionEvent FromPool(GameObject Actor, string Liquid = "water")
	{
		AllowLiquidCollectionEvent allowLiquidCollectionEvent = FromPool();
		allowLiquidCollectionEvent.Actor = Actor;
		allowLiquidCollectionEvent.Liquid = Liquid;
		allowLiquidCollectionEvent.LiquidVolume = null;
		allowLiquidCollectionEvent.Drams = 0;
		allowLiquidCollectionEvent.Skip = null;
		allowLiquidCollectionEvent.SkipList = null;
		allowLiquidCollectionEvent.Filter = null;
		allowLiquidCollectionEvent.Auto = false;
		allowLiquidCollectionEvent.ImpureOkay = false;
		allowLiquidCollectionEvent.SafeOnly = false;
		return allowLiquidCollectionEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor, string Liquid = "water")
	{
		if (!Object.Understood())
		{
			return false;
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			return Object.HandleEvent(FromPool(Actor, Liquid));
		}
		return true;
	}
}
