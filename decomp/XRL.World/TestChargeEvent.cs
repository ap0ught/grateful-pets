using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class TestChargeEvent : IChargeConsumptionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(TestChargeEvent), null, CountPool, ResetPool);

	private static List<TestChargeEvent> Pool;

	private static int PoolCounter;

	public TestChargeEvent()
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

	public static void ResetTo(ref TestChargeEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static TestChargeEvent FromPool()
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

	public static bool Check(GameObject Object, GameObject Source, int Amount, int Multiple = 1, long GridMask = 0L, bool Forced = false, bool LiveOnly = false, bool IncludeTransient = true, bool IncludeBiological = true)
	{
		if (Wanted(Object))
		{
			TestChargeEvent testChargeEvent = FromPool();
			testChargeEvent.Source = Source;
			testChargeEvent.Amount = Amount;
			testChargeEvent.StartingAmount = Amount;
			testChargeEvent.Multiple = Multiple;
			testChargeEvent.GridMask = GridMask;
			testChargeEvent.Forced = Forced;
			testChargeEvent.LiveOnly = LiveOnly;
			testChargeEvent.IncludeTransient = IncludeTransient;
			testChargeEvent.IncludeBiological = IncludeBiological;
			return !Process(Object, testChargeEvent);
		}
		return false;
	}

	public static bool Wanted(GameObject Object)
	{
		if (!Object.WantEvent(ID, IChargeEvent.CascadeLevel))
		{
			return Object.HasRegisteredEvent("TestCharge");
		}
		return true;
	}

	public static bool Process(GameObject Object, TestChargeEvent E)
	{
		if (!E.CheckRegisteredEvent(Object, "TestCharge"))
		{
			return false;
		}
		return Object.HandleEvent(E);
	}
}
