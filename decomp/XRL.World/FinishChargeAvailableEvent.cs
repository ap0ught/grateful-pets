using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class FinishChargeAvailableEvent : IFinalChargeProductionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(FinishChargeAvailableEvent), null, CountPool, ResetPool);

	private static List<FinishChargeAvailableEvent> Pool;

	private static int PoolCounter;

	public FinishChargeAvailableEvent()
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

	public static void ResetTo(ref FinishChargeAvailableEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static FinishChargeAvailableEvent FromPool()
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

	public static FinishChargeAvailableEvent FromPool(IChargeEvent From)
	{
		FinishChargeAvailableEvent finishChargeAvailableEvent = FromPool();
		finishChargeAvailableEvent.Source = From.Source;
		finishChargeAvailableEvent.Amount = From.Amount;
		finishChargeAvailableEvent.StartingAmount = From.StartingAmount;
		finishChargeAvailableEvent.Multiple = From.Multiple;
		finishChargeAvailableEvent.GridMask = From.GridMask;
		finishChargeAvailableEvent.Forced = From.Forced;
		finishChargeAvailableEvent.LiveOnly = From.LiveOnly;
		finishChargeAvailableEvent.IncludeTransient = From.IncludeTransient;
		finishChargeAvailableEvent.IncludeBiological = From.IncludeBiological;
		return finishChargeAvailableEvent;
	}

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool IncludeTransient = true, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			FinishChargeAvailableEvent E = FromPool();
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
			int used = E.Used;
			ResetTo(ref E);
			return used;
		}
		return 0;
	}

	public static int Send(GameObject Object, IChargeEvent From)
	{
		if (Wanted(Object))
		{
			FinishChargeAvailableEvent finishChargeAvailableEvent = FromPool(From);
			Process(Object, finishChargeAvailableEvent);
			return finishChargeAvailableEvent.Used;
		}
		return From.Used;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("FinishChargeAvailable");
		}
		return true;
	}

	public static bool Process(GameObject Object, FinishChargeAvailableEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "FinishChargeAvailable"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}
