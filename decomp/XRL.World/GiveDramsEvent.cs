using System;
using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GiveDramsEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GiveDramsEvent), null, CountPool, ResetPool);

	private static List<GiveDramsEvent> Pool;

	private static int PoolCounter;

	public const int PASSES = 5;

	public int Pass;

	public List<GameObject> StoredIn = new List<GameObject>();

	public GiveDramsEvent()
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

	public static void ResetTo(ref GiveDramsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GiveDramsEvent FromPool()
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

	public override void Reset()
	{
		base.Reset();
		Pass = 0;
		StoredIn.Clear();
	}

	public static GiveDramsEvent FromPool(GameObject Actor, string Liquid = "water", int Drams = 1, GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool Auto = false, bool SafeOnly = true, LiquidVolume LiquidVolume = null)
	{
		GiveDramsEvent giveDramsEvent = FromPool();
		giveDramsEvent.Actor = Actor;
		giveDramsEvent.Liquid = Liquid;
		giveDramsEvent.LiquidVolume = LiquidVolume;
		giveDramsEvent.Drams = Drams;
		giveDramsEvent.Skip = Skip;
		giveDramsEvent.SkipList = SkipList;
		giveDramsEvent.Filter = Filter;
		giveDramsEvent.Auto = Auto;
		giveDramsEvent.ImpureOkay = false;
		giveDramsEvent.SafeOnly = SafeOnly;
		giveDramsEvent.Pass = 0;
		giveDramsEvent.StoredIn.Clear();
		return giveDramsEvent;
	}

	public static bool Check(GameObject Actor, string Liquid = "water", int Drams = 1, GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool Auto = false, List<GameObject> StoredIn = null, bool SafeOnly = true, LiquidVolume LiquidVolume = null)
	{
		bool result = true;
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			GiveDramsEvent giveDramsEvent = FromPool(Actor, Liquid, Drams, Skip, SkipList, Filter, Auto, SafeOnly, LiquidVolume);
			for (int i = 1; i <= 5; i++)
			{
				giveDramsEvent.Pass = i;
				if (!Actor.HandleEvent(giveDramsEvent))
				{
					result = false;
					break;
				}
				if (giveDramsEvent.Drams <= 0)
				{
					break;
				}
			}
			StoredIn?.AddRange(giveDramsEvent.StoredIn);
		}
		return result;
	}
}
