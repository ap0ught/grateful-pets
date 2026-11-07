using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class UseDramsEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(UseDramsEvent), null, CountPool, ResetPool);

	private static List<UseDramsEvent> Pool;

	private static int PoolCounter;

	public const int PASSES = 3;

	public List<GameObject> TrackContainers;

	public bool Drinking;

	public int Pass;

	public UseDramsEvent()
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

	public static void ResetTo(ref UseDramsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static UseDramsEvent FromPool()
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
		TrackContainers = null;
		Drinking = false;
		Pass = 0;
	}

	public static UseDramsEvent FromPool(GameObject Actor, string Liquid = "water", int Drams = 1, GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool ImpureOkay = false, List<GameObject> TrackContainers = null, bool Drinking = false)
	{
		UseDramsEvent useDramsEvent = FromPool();
		useDramsEvent.Actor = Actor;
		useDramsEvent.Liquid = Liquid;
		useDramsEvent.LiquidVolume = null;
		useDramsEvent.Drams = Drams;
		useDramsEvent.Skip = Skip;
		useDramsEvent.SkipList = SkipList;
		useDramsEvent.Filter = Filter;
		useDramsEvent.Auto = false;
		useDramsEvent.ImpureOkay = ImpureOkay;
		useDramsEvent.SafeOnly = false;
		useDramsEvent.TrackContainers = TrackContainers;
		useDramsEvent.Drinking = Drinking;
		useDramsEvent.Pass = 0;
		return useDramsEvent;
	}

	public static bool Check(GameObject Actor, string Liquid = "water", int Drams = 1, GameObject Skip = null, List<GameObject> SkipList = null, Predicate<GameObject> Filter = null, bool ImpureOkay = false, List<GameObject> TrackContainers = null, bool Drinking = false)
	{
		if (Actor.WantEvent(ID, ILiquidEvent.CascadeLevel))
		{
			UseDramsEvent useDramsEvent = FromPool(Actor, Liquid, Drams, Skip, SkipList, Filter, ImpureOkay, TrackContainers, Drinking);
			for (int i = 1; i <= 3; i++)
			{
				useDramsEvent.Pass = i;
				if (!Actor.HandleEvent(useDramsEvent))
				{
					return false;
				}
				if (useDramsEvent.Drams <= 0)
				{
					break;
				}
			}
		}
		return true;
	}
}
