using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeMentalDefendEvent : IMentalAttackEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeMentalDefendEvent), null, CountPool, ResetPool);

	private static List<BeforeMentalDefendEvent> Pool;

	private static int PoolCounter;

	public BeforeMentalDefendEvent()
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

	public static void ResetTo(ref BeforeMentalDefendEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeMentalDefendEvent FromPool()
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

	public static BeforeMentalDefendEvent FromPool(IMentalAttackEvent PE)
	{
		BeforeMentalDefendEvent beforeMentalDefendEvent = FromPool();
		PE.ApplyTo(beforeMentalDefendEvent);
		return beforeMentalDefendEvent;
	}

	public static bool Check(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Defender.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			BeforeMentalDefendEvent beforeMentalDefendEvent = FromPool(PE);
			flag = PE.Defender.HandleEvent(beforeMentalDefendEvent, PE);
			PE.SetFrom(beforeMentalDefendEvent);
		}
		if (flag && PE.Defender.HasRegisteredEvent("BeforeMentalDefend"))
		{
			Event obj = Event.New("BeforeMentalDefend");
			PE.ApplyTo(obj);
			flag = PE.Defender.FireEvent(obj);
			PE.SetFrom(obj);
		}
		return flag;
	}
}
