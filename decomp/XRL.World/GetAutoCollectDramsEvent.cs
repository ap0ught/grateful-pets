using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetAutoCollectDramsEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetAutoCollectDramsEvent), null, CountPool, ResetPool);

	private static List<GetAutoCollectDramsEvent> Pool;

	private static int PoolCounter;

	public GetAutoCollectDramsEvent()
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

	public static void ResetTo(ref GetAutoCollectDramsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetAutoCollectDramsEvent FromPool()
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

	public static int GetFor(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			GetAutoCollectDramsEvent getAutoCollectDramsEvent = FromPool();
			getAutoCollectDramsEvent.Actor = Actor;
			getAutoCollectDramsEvent.Liquid = Liquid;
			getAutoCollectDramsEvent.LiquidVolume = null;
			getAutoCollectDramsEvent.Drams = 0;
			getAutoCollectDramsEvent.Skip = Skip;
			getAutoCollectDramsEvent.SkipList = SkipList;
			getAutoCollectDramsEvent.Filter = null;
			getAutoCollectDramsEvent.Auto = true;
			getAutoCollectDramsEvent.ImpureOkay = false;
			getAutoCollectDramsEvent.SafeOnly = false;
			Actor.HandleEvent(getAutoCollectDramsEvent);
			return getAutoCollectDramsEvent.Drams;
		}
		return 0;
	}
}
