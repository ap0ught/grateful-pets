using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeginMentalAttackEvent : IMentalAttackEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeginMentalAttackEvent), null, CountPool, ResetPool);

	private static List<BeginMentalAttackEvent> Pool;

	private static int PoolCounter;

	public BeginMentalAttackEvent()
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

	public static void ResetTo(ref BeginMentalAttackEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeginMentalAttackEvent FromPool()
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

	public static BeginMentalAttackEvent FromPool(IMentalAttackEvent PE)
	{
		BeginMentalAttackEvent beginMentalAttackEvent = FromPool();
		PE.ApplyTo(beginMentalAttackEvent);
		return beginMentalAttackEvent;
	}

	public static bool Check(IMentalAttackEvent PE)
	{
		bool flag = true;
		if (PE.Attacker.WantEvent(ID, IMentalAttackEvent.CascadeLevel))
		{
			BeginMentalAttackEvent beginMentalAttackEvent = FromPool(PE);
			flag = PE.Attacker.HandleEvent(beginMentalAttackEvent, PE);
			PE.SetFrom(beginMentalAttackEvent);
		}
		if (flag && PE.Attacker.HasRegisteredEvent("BeginMentalAttack"))
		{
			Event obj = Event.New("BeginMentalAttack");
			PE.ApplyTo(obj);
			flag = PE.Attacker.FireEvent(obj);
			PE.SetFrom(obj);
		}
		return flag;
	}
}
