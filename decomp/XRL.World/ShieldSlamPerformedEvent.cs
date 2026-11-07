using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 17)]
public class ShieldSlamPerformedEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ShieldSlamPerformedEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<ShieldSlamPerformedEvent> Pool;

	private static int PoolCounter;

	public GameObject Actor;

	public GameObject Target;

	public GameObject Shield;

	public int ShieldAV;

	public int Damage;

	public ShieldSlamPerformedEvent()
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

	public static void ResetTo(ref ShieldSlamPerformedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ShieldSlamPerformedEvent FromPool()
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
		ShieldAV = 0;
		Damage = 0;
	}

	public static void Send(GameObject Actor, GameObject Target, GameObject Shield, int ShieldAV, int Damage = 0)
	{
		if (true && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, CascadeLevel))
		{
			ShieldSlamPerformedEvent shieldSlamPerformedEvent = FromPool();
			shieldSlamPerformedEvent.Actor = Actor;
			shieldSlamPerformedEvent.Target = Target;
			shieldSlamPerformedEvent.Shield = Shield;
			shieldSlamPerformedEvent.ShieldAV = ShieldAV;
			shieldSlamPerformedEvent.Damage = Damage;
			Actor.HandleEvent(shieldSlamPerformedEvent);
		}
	}
}
