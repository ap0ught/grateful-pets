using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AnyAutoCollectDramsEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AnyAutoCollectDramsEvent), null, CountPool, ResetPool);

	private static List<AnyAutoCollectDramsEvent> Pool;

	private static int PoolCounter;

	public AnyAutoCollectDramsEvent()
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

	public static void ResetTo(ref AnyAutoCollectDramsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AnyAutoCollectDramsEvent FromPool()
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

	public static bool Check(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			AnyAutoCollectDramsEvent anyAutoCollectDramsEvent = FromPool();
			anyAutoCollectDramsEvent.Actor = Actor;
			anyAutoCollectDramsEvent.Liquid = Liquid;
			anyAutoCollectDramsEvent.LiquidVolume = null;
			anyAutoCollectDramsEvent.Drams = 0;
			anyAutoCollectDramsEvent.Skip = Skip;
			anyAutoCollectDramsEvent.SkipList = SkipList;
			anyAutoCollectDramsEvent.Filter = null;
			anyAutoCollectDramsEvent.Auto = true;
			anyAutoCollectDramsEvent.ImpureOkay = false;
			anyAutoCollectDramsEvent.SafeOnly = false;
			return !Actor.HandleEvent(anyAutoCollectDramsEvent);
		}
		return false;
	}
}
