using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class QueryChargeStorageEvent : IChargeStorageEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(QueryChargeStorageEvent), null, CountPool, ResetPool);

	private static List<QueryChargeStorageEvent> Pool;

	private static int PoolCounter;

	public QueryChargeStorageEvent()
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

	public static void ResetTo(ref QueryChargeStorageEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static QueryChargeStorageEvent FromPool()
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

	public static QueryChargeStorageEvent FromPool(IChargeStorageEvent From)
	{
		QueryChargeStorageEvent queryChargeStorageEvent = FromPool();
		queryChargeStorageEvent.Source = From.Source;
		queryChargeStorageEvent.Amount = From.Amount;
		queryChargeStorageEvent.StartingAmount = From.StartingAmount;
		queryChargeStorageEvent.Multiple = From.Multiple;
		queryChargeStorageEvent.GridMask = From.GridMask;
		queryChargeStorageEvent.Forced = From.Forced;
		queryChargeStorageEvent.LiveOnly = From.LiveOnly;
		queryChargeStorageEvent.IncludeTransient = From.IncludeTransient;
		queryChargeStorageEvent.IncludeBiological = From.IncludeBiological;
		queryChargeStorageEvent.Transient = From.Transient;
		queryChargeStorageEvent.UnlimitedTransient = From.UnlimitedTransient;
		return queryChargeStorageEvent;
	}

	public static int Retrieve(GameObject Object, GameObject Source, bool IncludeTransient = true, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			QueryChargeStorageEvent queryChargeStorageEvent = FromPool();
			queryChargeStorageEvent.Source = Source;
			queryChargeStorageEvent.Amount = 0;
			queryChargeStorageEvent.StartingAmount = 0;
			queryChargeStorageEvent.Multiple = 1;
			queryChargeStorageEvent.GridMask = GridMask;
			queryChargeStorageEvent.Forced = Forced;
			queryChargeStorageEvent.LiveOnly = LiveOnly;
			queryChargeStorageEvent.IncludeTransient = IncludeTransient;
			queryChargeStorageEvent.IncludeBiological = IncludeBiological;
			Process(Object, queryChargeStorageEvent);
			return Result(queryChargeStorageEvent);
		}
		return 0;
	}

	public static int Retrieve(out QueryChargeStorageEvent E, GameObject Object, GameObject Source, bool IncludeTransient = true, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeBiological = true)
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
			E.IncludeTransient = IncludeTransient;
			E.IncludeBiological = IncludeBiological;
			Process(Object, E);
			return Result(E);
		}
		E = null;
		return 0;
	}

	public static int Retrieve(GameObject Object, IChargeStorageEvent From)
	{
		if (Wanted(Object))
		{
			QueryChargeStorageEvent e = FromPool(From);
			Process(Object, e);
			return Result(e);
		}
		return 0;
	}

	public static int Retrieve(out QueryChargeStorageEvent E, GameObject Object, IChargeStorageEvent From)
	{
		if (Wanted(Object))
		{
			E = FromPool(From);
			Process(Object, E);
			return Result(E);
		}
		E = null;
		return 0;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("QueryChargeStorage");
		}
		return true;
	}

	public static bool Process(GameObject Object, QueryChargeStorageEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "QueryChargeStorage"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}

	public static int Result(QueryChargeStorageEvent E)
	{
		if (E.IncludeTransient)
		{
			if (E.UnlimitedTransient)
			{
				return int.MaxValue;
			}
			return E.Amount;
		}
		return E.Amount - E.Transient;
	}
}
