using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterMentalDefendEvent : IMentalAttackEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterMentalDefendEvent), null, CountPool, ResetPool);

	private static List<AfterMentalDefendEvent> Pool;

	private static int PoolCounter;

	public AfterMentalDefendEvent()
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

	public static void ResetTo(ref AfterMentalDefendEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterMentalDefendEvent FromPool()
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

	public static AfterMentalDefendEvent FromPool(IMentalAttackEvent PE)
	{
		AfterMentalDefendEvent afterMentalDefendEvent = FromPool();
		PE.ApplyTo(afterMentalDefendEvent);
		return afterMentalDefendEvent;
	}

	public static void Send(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Defender.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			AfterMentalDefendEvent afterMentalDefendEvent = FromPool(PE);
			flag = PE.Defender.HandleEvent(afterMentalDefendEvent, PE);
			PE.SetFrom(afterMentalDefendEvent);
		}
		if (flag && PE.Defender.HasRegisteredEvent("AfterMentalDefend"))
		{
			Event obj = Event.New("AfterMentalDefend");
			PE.ApplyTo(obj);
			flag = PE.Defender.FireEvent(obj);
			PE.SetFrom(obj);
		}
	}
}
