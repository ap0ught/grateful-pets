using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class QueryInductionChargeStorageEvent : IChargeStorageEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(QueryInductionChargeStorageEvent), null, CountPool, ResetPool);

	private static List<QueryInductionChargeStorageEvent> Pool;

	private static int PoolCounter;

	public QueryInductionChargeStorageEvent()
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

	public static void ResetTo(ref QueryInductionChargeStorageEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static QueryInductionChargeStorageEvent FromPool()
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

	public static QueryInductionChargeStorageEvent FromPool(IChargeStorageEvent From)
	{
		QueryInductionChargeStorageEvent queryInductionChargeStorageEvent = FromPool();
		queryInductionChargeStorageEvent.Source = From.Source;
		queryInductionChargeStorageEvent.Amount = From.Amount;
		queryInductionChargeStorageEvent.StartingAmount = From.StartingAmount;
		queryInductionChargeStorageEvent.Multiple = From.Multiple;
		queryInductionChargeStorageEvent.GridMask = From.GridMask;
		queryInductionChargeStorageEvent.Forced = From.Forced;
		queryInductionChargeStorageEvent.LiveOnly = From.LiveOnly;
		queryInductionChargeStorageEvent.IncludeTransient = From.IncludeTransient;
		queryInductionChargeStorageEvent.IncludeBiological = From.IncludeBiological;
		queryInductionChargeStorageEvent.Transient = From.Transient;
		queryInductionChargeStorageEvent.UnlimitedTransient = From.UnlimitedTransient;
		return queryInductionChargeStorageEvent;
	}

	public static int Retrieve(GameObject Object, GameObject Source, bool IncludeTransient = true, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			QueryInductionChargeStorageEvent queryInductionChargeStorageEvent = FromPool();
			queryInductionChargeStorageEvent.Source = Source;
			queryInductionChargeStorageEvent.Amount = 0;
			queryInductionChargeStorageEvent.StartingAmount = 0;
			queryInductionChargeStorageEvent.Multiple = 1;
			queryInductionChargeStorageEvent.GridMask = GridMask;
			queryInductionChargeStorageEvent.Forced = Forced;
			queryInductionChargeStorageEvent.LiveOnly = LiveOnly;
			queryInductionChargeStorageEvent.IncludeTransient = IncludeTransient;
			queryInductionChargeStorageEvent.IncludeBiological = IncludeBiological;
			Process(Object, queryInductionChargeStorageEvent);
			return Result(queryInductionChargeStorageEvent);
		}
		return 0;
	}

	public static int Retrieve(out QueryInductionChargeStorageEvent E, GameObject Object, GameObject Source, bool IncludeTransient = true, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeBiological = true)
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
			QueryInductionChargeStorageEvent e = FromPool(From);
			Process(Object, e);
			return Result(e);
		}
		return 0;
	}

	public static int Retrieve(out QueryInductionChargeStorageEvent E, GameObject Object, IChargeStorageEvent From)
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

	public static void Subprocess(GameObject Object, IChargeStorageEvent From)
	{
		if (Wanted(Object))
		{
			QueryInductionChargeStorageEvent queryInductionChargeStorageEvent = FromPool(From);
			Process(Object, queryInductionChargeStorageEvent);
			From.Amount = queryInductionChargeStorageEvent.Amount;
			From.Transient = queryInductionChargeStorageEvent.Transient;
			From.UnlimitedTransient = queryInductionChargeStorageEvent.UnlimitedTransient;
		}
	}

	public static void Subprocess(GameObject Object, IChargeStorageEvent From, ref int Available)
	{
		if (Available <= 0 || !Wanted(Object))
		{
			return;
		}
		QueryInductionChargeStorageEvent queryInductionChargeStorageEvent = FromPool(From);
		Process(Object, queryInductionChargeStorageEvent);
		if (From.Amount != queryInductionChargeStorageEvent.Amount)
		{
			int num = queryInductionChargeStorageEvent.Amount - From.Amount;
			int num2 = 1;
			if (num < 0)
			{
				num = -num;
				num2 = -1;
			}
			if (num > Available)
			{
				num = Available;
			}
			From.Amount += num * num2;
			int num3 = queryInductionChargeStorageEvent.Transient - From.Transient;
			int num4 = 1;
			if (num3 < 0)
			{
				num3 = -num3;
				num4 = -1;
			}
			if (num3 > Available)
			{
				num3 = Available;
			}
			From.Transient += num3 * num4;
			Available -= num;
		}
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("QueryInductionChargeStorage");
		}
		return true;
	}

	public static bool Process(GameObject Object, QueryInductionChargeStorageEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "QueryInductionChargeStorage"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}

	public static int Result(QueryInductionChargeStorageEvent E)
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
