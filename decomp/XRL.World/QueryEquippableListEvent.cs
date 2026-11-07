using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class QueryEquippableListEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(QueryEquippableListEvent), null, CountPool, ResetPool);

	private static List<QueryEquippableListEvent> Pool;

	private static int PoolCounter;

	public List<GameObject> List = new List<GameObject>();

	public string SlotType;

	public bool RequireDesirable;

	public bool RequirePossible;

	public QueryEquippableListEvent()
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

	public static void ResetTo(ref QueryEquippableListEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static QueryEquippableListEvent FromPool()
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
		List.Clear();
		SlotType = null;
		RequireDesirable = false;
		RequirePossible = false;
	}

	public static QueryEquippableListEvent FromPool(GameObject Actor, GameObject Item, string SlotType, bool RequireDesirable = false, bool RequirePossible = false)
	{
		QueryEquippableListEvent queryEquippableListEvent = FromPool();
		queryEquippableListEvent.Actor = Actor;
		queryEquippableListEvent.Item = Item;
		queryEquippableListEvent.SlotType = SlotType;
		queryEquippableListEvent.RequireDesirable = RequireDesirable;
		queryEquippableListEvent.RequirePossible = RequirePossible;
		return queryEquippableListEvent;
	}
}
