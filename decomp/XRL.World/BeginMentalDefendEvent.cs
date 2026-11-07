using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeginMentalDefendEvent : IMentalAttackEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeginMentalDefendEvent), null, CountPool, ResetPool);

	private static List<BeginMentalDefendEvent> Pool;

	private static int PoolCounter;

	public BeginMentalDefendEvent()
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

	public static void ResetTo(ref BeginMentalDefendEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeginMentalDefendEvent FromPool()
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

	public static BeginMentalDefendEvent FromPool(IMentalAttackEvent PE)
	{
		BeginMentalDefendEvent beginMentalDefendEvent = FromPool();
		PE.ApplyTo(beginMentalDefendEvent);
		return beginMentalDefendEvent;
	}

	public static bool Check(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Defender.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			BeginMentalDefendEvent beginMentalDefendEvent = FromPool(PE);
			flag = PE.Defender.HandleEvent(beginMentalDefendEvent, PE);
			PE.SetFrom(beginMentalDefendEvent);
		}
		if (flag && PE.Defender.HasRegisteredEvent("BeginMentalDefend"))
		{
			Event obj = Event.New("BeginMentalDefend");
			PE.ApplyTo(obj);
			flag = PE.Defender.FireEvent(obj);
			PE.SetFrom(obj);
		}
		return flag;
	}
}
