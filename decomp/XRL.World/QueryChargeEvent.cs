using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class QueryChargeEvent : IChargeConsumptionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(QueryChargeEvent), null, CountPool, ResetPool);

	private static List<QueryChargeEvent> Pool;

	private static int PoolCounter;

	public QueryChargeEvent()
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

	public static void ResetTo(ref QueryChargeEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static QueryChargeEvent FromPool()
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

	public static QueryChargeEvent FromPool(IChargeEvent From)
	{
		QueryChargeEvent queryChargeEvent = FromPool();
		queryChargeEvent.Source = From.Source;
		queryChargeEvent.Amount = From.Amount;
		queryChargeEvent.StartingAmount = From.StartingAmount;
		queryChargeEvent.Multiple = From.Multiple;
		queryChargeEvent.GridMask = From.GridMask;
		queryChargeEvent.Forced = From.Forced;
		queryChargeEvent.LiveOnly = From.LiveOnly;
		queryChargeEvent.IncludeTransient = From.IncludeTransient;
		queryChargeEvent.IncludeBiological = From.IncludeBiological;
		return queryChargeEvent;
	}

	public static int Retrieve(GameObject Object, GameObject Source, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			QueryChargeEvent queryChargeEvent = FromPool();
			queryChargeEvent.Source = Source;
			queryChargeEvent.Amount = 0;
			queryChargeEvent.StartingAmount = 0;
			queryChargeEvent.Multiple = Multiple;
			queryChargeEvent.GridMask = GridMask;
			queryChargeEvent.Forced = Forced;
			queryChargeEvent.LiveOnly = LiveOnly;
			queryChargeEvent.IncludeTransient = IncludeTransient;
			queryChargeEvent.IncludeBiological = IncludeBiological;
			Process(Object, queryChargeEvent);
			return queryChargeEvent.Amount;
		}
		return 0;
	}

	public static int Retrieve(out QueryChargeEvent E, GameObject Object, GameObject Source, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			E = FromPool();
			E.Source = Source;
			E.Amount = 0;
			E.StartingAmount = 0;
			E.Multiple = Multiple;
			E.GridMask = GridMask;
			E.Forced = Forced;
			E.LiveOnly = LiveOnly;
			E.IncludeTransient = IncludeTransient;
			E.IncludeBiological = IncludeBiological;
			Process(Object, E);
			return E.Amount;
		}
		E = null;
		return 0;
	}

	public static int Retrieve(GameObject Object, IChargeEvent From)
	{
		if (Wanted(Object))
		{
			QueryChargeEvent queryChargeEvent = FromPool(From);
			Process(Object, queryChargeEvent);
			return queryChargeEvent.Amount;
		}
		return 0;
	}

	public static int Retrieve(out QueryChargeEvent E, GameObject Object, IChargeEvent From)
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
			return Object.HasRegisteredEvent("QueryCharge");
		}
		return true;
	}

	public static bool Process(GameObject Object, QueryChargeEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "QueryCharge"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}
