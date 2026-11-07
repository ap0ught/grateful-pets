using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class RechargeAvailableEvent : IInitialChargeProductionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(RechargeAvailableEvent), null, CountPool, ResetPool);

	private static List<RechargeAvailableEvent> Pool;

	private static int PoolCounter;

	public RechargeAvailableEvent()
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

	public static void ResetTo(ref RechargeAvailableEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static RechargeAvailableEvent FromPool()
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

	public static RechargeAvailableEvent FromPool(IChargeEvent From)
	{
		RechargeAvailableEvent rechargeAvailableEvent = FromPool();
		rechargeAvailableEvent.Source = From.Source;
		rechargeAvailableEvent.Amount = From.Amount;
		rechargeAvailableEvent.StartingAmount = From.Amount;
		rechargeAvailableEvent.Multiple = From.Multiple;
		rechargeAvailableEvent.GridMask = From.GridMask;
		rechargeAvailableEvent.Forced = From.Forced;
		rechargeAvailableEvent.LiveOnly = From.LiveOnly;
		rechargeAvailableEvent.IncludeTransient = From.IncludeTransient;
		rechargeAvailableEvent.IncludeBiological = From.IncludeBiological;
		return rechargeAvailableEvent;
	}

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			RechargeAvailableEvent rechargeAvailableEvent = FromPool();
			rechargeAvailableEvent.Source = Source;
			rechargeAvailableEvent.Amount = Amount;
			rechargeAvailableEvent.StartingAmount = Amount;
			rechargeAvailableEvent.Multiple = Multiple;
			rechargeAvailableEvent.GridMask = GridMask;
			rechargeAvailableEvent.Forced = Forced;
			rechargeAvailableEvent.LiveOnly = false;
			rechargeAvailableEvent.IncludeTransient = IncludeTransient;
			rechargeAvailableEvent.IncludeBiological = IncludeBiological;
			Process(Object, rechargeAvailableEvent);
			return rechargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(out RechargeAvailableEvent E, GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			E = FromPool();
			E.Source = Source;
			E.Amount = Amount;
			E.StartingAmount = Amount;
			E.Multiple = Multiple;
			E.GridMask = GridMask;
			E.Forced = Forced;
			E.LiveOnly = false;
			E.IncludeTransient = IncludeTransient;
			E.IncludeBiological = IncludeBiological;
			Process(Object, E);
			return E.Used;
		}
		E = null;
		return 0;
	}

	public static int Send(GameObject Object, IChargeEvent From)
	{
		if (Wanted(Object))
		{
			RechargeAvailableEvent rechargeAvailableEvent = FromPool(From);
			Process(Object, rechargeAvailableEvent);
			return rechargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(GameObject Object, IChargeEvent From, int Amount)
	{
		if (Wanted(Object))
		{
			RechargeAvailableEvent rechargeAvailableEvent = FromPool(From);
			rechargeAvailableEvent.Amount = Amount;
			Process(Object, rechargeAvailableEvent);
			return rechargeAvailableEvent.Used;
		}
		return 0;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("RechargeAvailable");
		}
		return true;
	}

	public static bool Process(GameObject Object, RechargeAvailableEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "RechargeAvailable"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}
