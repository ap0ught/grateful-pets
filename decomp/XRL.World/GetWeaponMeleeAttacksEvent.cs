using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetWeaponMeleeAttacksEvent : IMeleeAttackEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetWeaponMeleeAttacksEvent), null, CountPool, ResetPool);

	private static List<GetWeaponMeleeAttacksEvent> Pool;

	private static int PoolCounter;

	public GameObject Weapon;

	public bool Primary;

	public bool Intrinsic;

	public GetWeaponMeleeAttacksEvent()
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

	public static void ResetTo(ref GetWeaponMeleeAttacksEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetWeaponMeleeAttacksEvent FromPool()
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

	public override void Reset()
	{
		base.Reset();
		Weapon = null;
		Primary = false;
		Intrinsic = false;
	}

	public static GetWeaponMeleeAttacksEvent HandleFrom(GameObject Actor, GameObject Weapon, bool Primary = false, bool Intrinsic = false)
	{
		GetWeaponMeleeAttacksEvent getWeaponMeleeAttacksEvent = FromPool();
		getWeaponMeleeAttacksEvent.Actor = Actor;
		getWeaponMeleeAttacksEvent.Weapon = Weapon;
		getWeaponMeleeAttacksEvent.Primary = Primary;
		getWeaponMeleeAttacksEvent.Intrinsic = Intrinsic;
		if (Actor.HandleEvent(getWeaponMeleeAttacksEvent))
		{
			Weapon.HandleEvent(getWeaponMeleeAttacksEvent);
		}
		return getWeaponMeleeAttacksEvent;
	}
}
