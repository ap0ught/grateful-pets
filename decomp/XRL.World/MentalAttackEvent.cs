using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class MentalAttackEvent : IMentalAttackEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(MentalAttackEvent), null, CountPool, ResetPool);

	private static List<MentalAttackEvent> Pool;

	private static int PoolCounter;

	public MentalAttackEvent()
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

	public static void ResetTo(ref MentalAttackEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static MentalAttackEvent FromPool()
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

	public static MentalAttackEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Source = null, string Command = null, string Dice = null, int Type = 0, int Magnitude = int.MinValue)
	{
		MentalAttackEvent mentalAttackEvent = FromPool();
		mentalAttackEvent.Attacker = Attacker;
		mentalAttackEvent.Defender = Defender;
		mentalAttackEvent.Source = Source;
		mentalAttackEvent.Command = Command;
		mentalAttackEvent.Dice = Dice;
		mentalAttackEvent.Type = Type;
		mentalAttackEvent.Magnitude = Magnitude;
		return mentalAttackEvent;
	}
}
