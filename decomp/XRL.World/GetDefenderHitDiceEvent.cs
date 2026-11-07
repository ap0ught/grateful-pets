using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetDefenderHitDiceEvent : IHitDiceEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetDefenderHitDiceEvent), null, CountPool, ResetPool);

	private static List<GetDefenderHitDiceEvent> Pool;

	private static int PoolCounter;

	public GetDefenderHitDiceEvent()
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

	public static void ResetTo(ref GetDefenderHitDiceEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetDefenderHitDiceEvent FromPool()
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

	public static GetDefenderHitDiceEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, int PenetrationBonus, int AV, bool ShieldBlocked)
	{
		GetDefenderHitDiceEvent getDefenderHitDiceEvent = FromPool();
		getDefenderHitDiceEvent.Attacker = Attacker;
		getDefenderHitDiceEvent.Defender = Defender;
		getDefenderHitDiceEvent.Weapon = Weapon;
		getDefenderHitDiceEvent.PenetrationBonus = PenetrationBonus;
		getDefenderHitDiceEvent.AV = AV;
		getDefenderHitDiceEvent.ShieldBlocked = ShieldBlocked;
		return getDefenderHitDiceEvent;
	}

	public static bool Process(ref int PenetrationBonus, ref int AV, ref bool ShieldBlocked, GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return IHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon, "GetDefenderHitDice", Defender, ID, IHitDiceEvent.CascadeLevel, FromPool);
	}
}
