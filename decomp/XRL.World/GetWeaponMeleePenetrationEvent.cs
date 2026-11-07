using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Pool)]
public class GetWeaponMeleePenetrationEvent : IMeleePenetrationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetWeaponMeleePenetrationEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 0;

	private static List<GetWeaponMeleePenetrationEvent> Pool;

	private static int PoolCounter;

	public GetWeaponMeleePenetrationEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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

	public static void ResetTo(ref GetWeaponMeleePenetrationEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetWeaponMeleePenetrationEvent FromPool()
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

	public static bool Process(ref int Penetrations, ref int StatBonus, ref int MaxStatBonus, ref int PenetrationBonus, ref int MaxPenetrationBonus, int AV, bool Critical, string Properties, string Hand, GameObject Attacker, GameObject Defender, GameObject Weapon)
	{
		return IMeleePenetrationEvent.Process(ref Penetrations, ref StatBonus, ref MaxStatBonus, ref PenetrationBonus, ref MaxPenetrationBonus, AV, Critical, Properties, Hand, Attacker, Defender, Weapon, "GetWeaponPenModifier", Weapon, ID, CascadeLevel, FromPool);
	}
}
