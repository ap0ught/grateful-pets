using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterMentalAttackEvent : IMentalAttackEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterMentalAttackEvent), null, CountPool, ResetPool);

	private static List<AfterMentalAttackEvent> Pool;

	private static int PoolCounter;

	public AfterMentalAttackEvent()
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

	public static void ResetTo(ref AfterMentalAttackEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterMentalAttackEvent FromPool()
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

	public static AfterMentalAttackEvent FromPool(IMentalAttackEvent PE)
	{
		AfterMentalAttackEvent afterMentalAttackEvent = FromPool();
		PE.ApplyTo(afterMentalAttackEvent);
		return afterMentalAttackEvent;
	}

	public static void Send(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Attacker.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			AfterMentalAttackEvent afterMentalAttackEvent = FromPool(PE);
			flag = PE.Attacker.HandleEvent(afterMentalAttackEvent, PE);
			PE.SetFrom(afterMentalAttackEvent);
		}
		if (flag && PE.Attacker.HasRegisteredEvent("AfterMentalAttack"))
		{
			Event obj = Event.New("AfterMentalAttack");
			PE.ApplyTo(obj);
			flag = PE.Attacker.FireEvent(obj);
			PE.SetFrom(obj);
		}
	}
}
