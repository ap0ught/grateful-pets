using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ChargeAvailableEvent : IInitialChargeProductionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ChargeAvailableEvent), null, CountPool, ResetPool);

	private static List<ChargeAvailableEvent> Pool;

	private static int PoolCounter;

	public ChargeAvailableEvent()
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

	public static void ResetTo(ref ChargeAvailableEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ChargeAvailableEvent FromPool()
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

	public static int Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false)
	{
		if (Wanted(Object))
		{
			ChargeAvailableEvent chargeAvailableEvent = FromPool();
			chargeAvailableEvent.Source = Source;
			chargeAvailableEvent.Amount = Amount;
			chargeAvailableEvent.StartingAmount = Amount;
			chargeAvailableEvent.Multiple = Multiple;
			chargeAvailableEvent.GridMask = GridMask;
			chargeAvailableEvent.Forced = Forced;
			chargeAvailableEvent.LiveOnly = false;
			Process(Object, chargeAvailableEvent);
			return chargeAvailableEvent.Used;
		}
		return 0;
	}

	public static int Send(out ChargeAvailableEvent E, GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false)
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
			Process(Object, E);
			int used = E.Used;
			ResetTo(ref E);
			return used;
		}
		E = null;
		return 0;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("ChargeAvailable");
		}
		return true;
	}

	public static bool Process(GameObject Object, ChargeAvailableEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "ChargeAvailable"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}
