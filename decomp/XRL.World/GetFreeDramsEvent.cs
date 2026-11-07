using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetFreeDramsEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetFreeDramsEvent), null, CountPool, ResetPool);

	private static List<GetFreeDramsEvent> Pool;

	private static int PoolCounter;

	public GetFreeDramsEvent()
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

	public static void ResetTo(ref GetFreeDramsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetFreeDramsEvent FromPool()
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

	public static GetFreeDramsEvent FromPool(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool ImpureOkay = false)
	{
		GetFreeDramsEvent getFreeDramsEvent = FromPool();
		getFreeDramsEvent.Actor = Actor;
		getFreeDramsEvent.Liquid = Liquid;
		getFreeDramsEvent.LiquidVolume = null;
		getFreeDramsEvent.Drams = 0;
		getFreeDramsEvent.Skip = Skip;
		getFreeDramsEvent.SkipList = SkipList;
		getFreeDramsEvent.Filter = Filter;
		getFreeDramsEvent.Auto = false;
		getFreeDramsEvent.ImpureOkay = ImpureOkay;
		getFreeDramsEvent.SafeOnly = false;
		return getFreeDramsEvent;
	}

	public static int GetFor(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool ImpureOkay = false)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			GetFreeDramsEvent getFreeDramsEvent = FromPool(Actor, Liquid, Skip, SkipList, Filter, ImpureOkay);
			Actor.HandleEvent(getFreeDramsEvent);
			return getFreeDramsEvent.Drams;
		}
		return 0;
	}
}
