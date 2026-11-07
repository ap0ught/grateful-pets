using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 17)]
public class GetShieldSlamDamageEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetShieldSlamDamageEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<GetShieldSlamDamageEvent> Pool;

	private static int PoolCounter;

	public GameObject Actor;

	public GameObject Target;

	public GameObject Shield;

	public string Attributes;

	public string ExtraDesc;

	public int Damage;

	public int ShieldAV;

	public int StrengthMod;

	public bool Prospective;

	public GetShieldSlamDamageEvent()
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

	public static void ResetTo(ref GetShieldSlamDamageEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetShieldSlamDamageEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Target = null;
		Shield = null;
		Attributes = null;
		ExtraDesc = null;
		Damage = 0;
		ShieldAV = 0;
		StrengthMod = 0;
		Prospective = false;
	}

	public static void GetFor(GameObject Actor, GameObject Target, GameObject Shield, ref string Attributes, ref string ExtraDesc, ref int Damage, int ShieldAV, int StrengthMod, bool Prospective = false)
	{
		if (true && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			GetShieldSlamDamageEvent getShieldSlamDamageEvent = FromPool();
			getShieldSlamDamageEvent.Actor = Actor;
			getShieldSlamDamageEvent.Target = Target;
			getShieldSlamDamageEvent.Shield = Shield;
			getShieldSlamDamageEvent.Attributes = Attributes;
			getShieldSlamDamageEvent.ExtraDesc = ExtraDesc;
			getShieldSlamDamageEvent.Damage = Damage;
			getShieldSlamDamageEvent.ShieldAV = ShieldAV;
			getShieldSlamDamageEvent.StrengthMod = StrengthMod;
			getShieldSlamDamageEvent.Prospective = Prospective;
			Actor.HandleEvent(getShieldSlamDamageEvent);
			Attributes = getShieldSlamDamageEvent.Attributes;
			Damage = getShieldSlamDamageEvent.Damage;
			ExtraDesc = getShieldSlamDamageEvent.ExtraDesc;
		}
	}
}
