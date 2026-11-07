using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class QueryChargeProductionEvent : IChargeEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(QueryChargeProductionEvent), null, CountPool, ResetPool);

	private static List<QueryChargeProductionEvent> Pool;

	private static int PoolCounter;

	public QueryChargeProductionEvent()
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

	public static void ResetTo(ref QueryChargeProductionEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static QueryChargeProductionEvent FromPool()
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

	public static QueryChargeProductionEvent FromPool(QueryChargeProductionEvent From)
	{
		QueryChargeProductionEvent queryChargeProductionEvent = FromPool();
		queryChargeProductionEvent.Source = From.Source;
		queryChargeProductionEvent.Amount = From.Amount;
		queryChargeProductionEvent.StartingAmount = From.StartingAmount;
		queryChargeProductionEvent.Multiple = From.Multiple;
		queryChargeProductionEvent.GridMask = From.GridMask;
		queryChargeProductionEvent.Forced = From.Forced;
		queryChargeProductionEvent.LiveOnly = From.LiveOnly;
		queryChargeProductionEvent.IncludeTransient = From.IncludeTransient;
		queryChargeProductionEvent.IncludeBiological = From.IncludeBiological;
		return queryChargeProductionEvent;
	}

	public static int Retrieve(GameObject Object, GameObject Source, long GridMask = 0L, bool Forced = false, bool LiveOnly = false)
	{
		if (Wanted(Object))
		{
			QueryChargeProductionEvent queryChargeProductionEvent = FromPool();
			queryChargeProductionEvent.Source = Source;
			queryChargeProductionEvent.Amount = 0;
			queryChargeProductionEvent.StartingAmount = 0;
			queryChargeProductionEvent.Multiple = 1;
			queryChargeProductionEvent.GridMask = GridMask;
			queryChargeProductionEvent.Forced = Forced;
			queryChargeProductionEvent.LiveOnly = LiveOnly;
			queryChargeProductionEvent.IncludeTransient = true;
			queryChargeProductionEvent.IncludeBiological = true;
			Process(Object, queryChargeProductionEvent);
			return queryChargeProductionEvent.Amount;
		}
		return 0;
	}

	public static int Retrieve(out QueryChargeProductionEvent E, GameObject Object, GameObject Source, long GridMask = 0L, bool Forced = false, bool LiveOnly = false)
	{
		if (Wanted(Object))
		{
			E = FromPool();
			E.Source = Source;
			E.Amount = 0;
			E.StartingAmount = 0;
			E.Multiple = 1;
			E.GridMask = GridMask;
			E.Forced = Forced;
			E.LiveOnly = LiveOnly;
			E.IncludeTransient = true;
			E.IncludeBiological = true;
			Process(Object, E);
			return E.Amount;
		}
		E = null;
		return 0;
	}

	public static int Retrieve(GameObject Object, QueryChargeProductionEvent From)
	{
		if (Wanted(Object))
		{
			QueryChargeProductionEvent queryChargeProductionEvent = FromPool(From);
			Process(Object, queryChargeProductionEvent);
			return queryChargeProductionEvent.Amount;
		}
		return 0;
	}

	public static int Retrieve(out QueryChargeProductionEvent E, GameObject Object, QueryChargeProductionEvent From)
	{
		if (Wanted(Object))
		{
			E = FromPool(From);
			Process(Object, E);
			return E.Amount;
		}
		E = null;
		return 0;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("QueryChargeProduction");
		}
		return true;
	}

	public static bool Process(GameObject Object, QueryChargeProductionEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "QueryChargeProduction"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}
