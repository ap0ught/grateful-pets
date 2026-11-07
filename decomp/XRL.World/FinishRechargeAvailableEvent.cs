using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class FinishRechargeAvailableEvent : IFinalChargeProductionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(FinishRechargeAvailableEvent), null, CountPool, ResetPool);

	private static List<FinishRechargeAvailableEvent> Pool;

	private static int PoolCounter;

	public FinishRechargeAvailableEvent()
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

	public static void ResetTo(ref FinishRechargeAvailableEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static FinishRechargeAvailableEvent FromPool()
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

	public static FinishRechargeAvailableEvent FromPool(IChargeEvent From)
	{
		FinishRechargeAvailableEvent finishRechargeAvailableEvent = FromPool();
		finishRechargeAvailableEvent.Source = From.Source;
		finishRechargeAvailableEvent.Amount = From.Amount;
		finishRechargeAvailableEvent.StartingAmount = From.StartingAmount;
		finishRechargeAvailableEvent.Multiple = From.Multiple;
		finishRechargeAvailableEvent.GridMask = From.GridMask;
		finishRechargeAvailableEvent.Forced = From.Forced;
		finishRechargeAvailableEvent.LiveOnly = From.LiveOnly;
		finishRechargeAvailableEvent.IncludeTransient = From.IncludeTransient;
		finishRechargeAvailableEvent.IncludeBiological = From.IncludeBiological;
		return finishRechargeAvailableEvent;
	}

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = false, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			FinishRechargeAvailableEvent finishRechargeAvailableEvent = FromPool();
			finishRechargeAvailableEvent.Source = Source;
			finishRechargeAvailableEvent.Amount = Amount;
			finishRechargeAvailableEvent.StartingAmount = Amount;
			finishRechargeAvailableEvent.Multiple = Multiple;
			finishRechargeAvailableEvent.GridMask = GridMask;
			finishRechargeAvailableEvent.Forced = Forced;
			finishRechargeAvailableEvent.LiveOnly = false;
			finishRechargeAvailableEvent.IncludeTransient = IncludeTransient;
			finishRechargeAvailableEvent.IncludeBiological = IncludeBiological;
			Process(Object, finishRechargeAvailableEvent);
			return finishRechargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(GameObject Object, IChargeEvent From)
	{
		if (Wanted(Object))
		{
			FinishRechargeAvailableEvent finishRechargeAvailableEvent = FromPool(From);
			Process(Object, finishRechargeAvailableEvent);
			return finishRechargeAvailableEvent.Used;
		}
		return From.Used;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("FinishRechargeAvailable");
		}
		return true;
	}

	public static bool Process(GameObject Object, FinishRechargeAvailableEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "FinishRechargeAvailable"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}
