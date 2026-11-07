using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetAttackerHitDiceEvent : IHitDiceEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetAttackerHitDiceEvent), null, CountPool, ResetPool);

	private static List<GetAttackerHitDiceEvent> Pool;

	private static int PoolCounter;

	public GetAttackerHitDiceEvent()
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

	public static void ResetTo(ref GetAttackerHitDiceEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetAttackerHitDiceEvent FromPool()
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

	public static GetAttackerHitDiceEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, int PenetrationBonus, int AV, bool ShieldBlocked)
	{
		GetAttackerHitDiceEvent getAttackerHitDiceEvent = FromPool();
		getAttackerHitDiceEvent.Attacker = Attacker;
		getAttackerHitDiceEvent.Defender = Defender;
		getAttackerHitDiceEvent.Weapon = Weapon;
		getAttackerHitDiceEvent.PenetrationBonus = PenetrationBonus;
		getAttackerHitDiceEvent.AV = AV;
		getAttackerHitDiceEvent.ShieldBlocked = ShieldBlocked;
		return getAttackerHitDiceEvent;
	}

	public static bool Process(ref int PenetrationBonus, ref int AV, ref bool ShieldBlocked, GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return IHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon, "GetAttackerHitDice", Attacker, ID, IHitDiceEvent.CascadeLevel, FromPool);
	}
}
