using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetStorableDramsEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetStorableDramsEvent), null, CountPool, ResetPool);

	private static List<GetStorableDramsEvent> Pool;

	private static int PoolCounter;

	public GetStorableDramsEvent()
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

	public static void ResetTo(ref GetStorableDramsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetStorableDramsEvent FromPool()
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

	public static GetStorableDramsEvent FromPool(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool SafeOnly = true, LiquidVolume LiquidVolume = null)
	{
		GetStorableDramsEvent getStorableDramsEvent = FromPool();
		getStorableDramsEvent.Actor = Actor;
		getStorableDramsEvent.Liquid = Liquid;
		getStorableDramsEvent.LiquidVolume = LiquidVolume;
		getStorableDramsEvent.Drams = 0;
		getStorableDramsEvent.Skip = Skip;
		getStorableDramsEvent.SkipList = SkipList;
		getStorableDramsEvent.Filter = Filter;
		getStorableDramsEvent.Auto = false;
		getStorableDramsEvent.ImpureOkay = false;
		getStorableDramsEvent.SafeOnly = SafeOnly;
		return getStorableDramsEvent;
	}

	public static int GetFor(GameObject Actor, string Liquid = "water", GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool SafeOnly = true, LiquidVolume LiquidVolume = null)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			GetStorableDramsEvent getStorableDramsEvent = FromPool(Actor, Liquid, Skip, SkipList, Filter, SafeOnly, LiquidVolume);
			Actor.HandleEvent(getStorableDramsEvent);
			return getStorableDramsEvent.Drams;
		}
		return 0;
	}
}
