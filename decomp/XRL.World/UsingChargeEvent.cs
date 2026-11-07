using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class UsingChargeEvent : IChargeConsumptionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(UsingChargeEvent), null, CountPool, ResetPool);

	private static List<UsingChargeEvent> Pool;

	private static int PoolCounter;

	public UsingChargeEvent()
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

	public static void ResetTo(ref UsingChargeEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static UsingChargeEvent FromPool()
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

	public static void Send(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true, int PowerLoadLevel = 100)
	{
		if (Wanted(Object))
		{
			UsingChargeEvent usingChargeEvent = FromPool();
			usingChargeEvent.Source = Source;
			usingChargeEvent.Amount = Amount;
			usingChargeEvent.StartingAmount = Amount;
			usingChargeEvent.Multiple = Multiple;
			usingChargeEvent.GridMask = GridMask;
			usingChargeEvent.Forced = Forced;
			usingChargeEvent.LiveOnly = LiveOnly;
			usingChargeEvent.IncludeTransient = IncludeTransient;
			usingChargeEvent.IncludeBiological = IncludeBiological;
			usingChargeEvent.PowerLoadLevel = PowerLoadLevel;
			Process(Object, usingChargeEvent);
		}
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("UsingCharge");
		}
		return true;
	}

	public static void Process(GameObject Object, UsingChargeEvent E)
	{
		if (E.CheckRegisteredEvent(Object, "UsingCharge"))
		{
			Object.HandleEvent(E);
		}
	}
}
