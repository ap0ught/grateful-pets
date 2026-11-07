using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Pool)]
public class WantsLiquidCollectionEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(WantsLiquidCollectionEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 0;

	private static List<WantsLiquidCollectionEvent> Pool;

	private static int PoolCounter;

	public WantsLiquidCollectionEvent()
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

	public static void ResetTo(ref WantsLiquidCollectionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static WantsLiquidCollectionEvent FromPool()
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

	public static WantsLiquidCollectionEvent FromPool(GameObject Actor, string Liquid = "water")
	{
		WantsLiquidCollectionEvent wantsLiquidCollectionEvent = FromPool();
		wantsLiquidCollectionEvent.Actor = Actor;
		wantsLiquidCollectionEvent.Liquid = Liquid;
		wantsLiquidCollectionEvent.LiquidVolume = null;
		wantsLiquidCollectionEvent.Drams = 0;
		wantsLiquidCollectionEvent.Skip = null;
		wantsLiquidCollectionEvent.SkipList = null;
		wantsLiquidCollectionEvent.Filter = null;
		wantsLiquidCollectionEvent.Auto = false;
		wantsLiquidCollectionEvent.ImpureOkay = false;
		wantsLiquidCollectionEvent.SafeOnly = false;
		return wantsLiquidCollectionEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor, string Liquid = "water")
	{
		if (!Object.Understood())
		{
			return false;
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			return !Object.HandleEvent(FromPool(Actor, Liquid));
		}
		return false;
	}
}
