using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetWeaponHitDiceEvent : IHitDiceEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetWeaponHitDiceEvent), null, CountPool, ResetPool);

	private static List<GetWeaponHitDiceEvent> Pool;

	private static int PoolCounter;

	public GetWeaponHitDiceEvent()
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

	public static void ResetTo(ref GetWeaponHitDiceEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetWeaponHitDiceEvent FromPool()
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

	public static GetWeaponHitDiceEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Weapon, int PenetrationBonus, int AV, bool ShieldBlocked)
	{
		GetWeaponHitDiceEvent getWeaponHitDiceEvent = FromPool();
		getWeaponHitDiceEvent.Attacker = Attacker;
		getWeaponHitDiceEvent.Defender = Defender;
		getWeaponHitDiceEvent.Weapon = Weapon;
		getWeaponHitDiceEvent.PenetrationBonus = PenetrationBonus;
		getWeaponHitDiceEvent.AV = AV;
		getWeaponHitDiceEvent.ShieldBlocked = ShieldBlocked;
		return getWeaponHitDiceEvent;
	}

	public static bool Process(ref int PenetrationBonus, ref int AV, ref bool ShieldBlocked, GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return IHitDiceEvent.Process(ref PenetrationBonus, ref AV, ref ShieldBlocked, Attacker, Defender, Weapon, "GetWeaponHitDice", Weapon, ID, IHitDiceEvent.CascadeLevel, FromPool);
	}
}
