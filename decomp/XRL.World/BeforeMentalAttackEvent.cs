using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeMentalAttackEvent : IMentalAttackEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeMentalAttackEvent), null, CountPool, ResetPool);

	private static List<BeforeMentalAttackEvent> Pool;

	private static int PoolCounter;

	public BeforeMentalAttackEvent()
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

	public static void ResetTo(ref BeforeMentalAttackEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeMentalAttackEvent FromPool()
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

	public static BeforeMentalAttackEvent FromPool(IMentalAttackEvent PE)
	{
		BeforeMentalAttackEvent beforeMentalAttackEvent = FromPool();
		PE.ApplyTo(beforeMentalAttackEvent);
		return beforeMentalAttackEvent;
	}

	public static bool Check(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Attacker.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			BeforeMentalAttackEvent beforeMentalAttackEvent = FromPool(PE);
			flag = PE.Attacker.HandleEvent(beforeMentalAttackEvent, PE);
			PE.SetFrom(beforeMentalAttackEvent);
		}
		if (flag && PE.Attacker.HasRegisteredEvent("BeforeMentalAttack"))
		{
			Event obj = Event.New("BeforeMentalAttack");
			PE.ApplyTo(obj);
			flag = PE.Attacker.FireEvent(obj);
			PE.SetFrom(obj);
		}
		return flag;
	}
}
