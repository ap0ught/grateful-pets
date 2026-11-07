using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AttackerDealtDamageEvent : IDamageEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AttackerDealtDamageEvent), null, CountPool, ResetPool);

	private static List<AttackerDealtDamageEvent> Pool;

	private static int PoolCounter;

	public AttackerDealtDamageEvent()
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

	public static void ResetTo(ref AttackerDealtDamageEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AttackerDealtDamageEvent FromPool()
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

	public static void Send(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AttackerDealtDamage"))
		{
			Event obj = Event.New("AttackerDealtDamage");
			obj.SetParameter("Damage", Damage);
			obj.SetParameter("Object", Object);
			obj.SetParameter("Owner", Actor);
			obj.SetParameter("Source", Source);
			obj.SetParameter("Weapon", Weapon);
			obj.SetParameter("Projectile", Projectile);
			obj.SetFlag("Indirect", Indirect);
			ParentEvent?.PreprocessChildEvent(obj);
			flag = Actor.FireEvent(obj, ParentEvent);
			ParentEvent?.ProcessChildEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AttackerDealtDamageEvent attackerDealtDamageEvent = FromPool();
			attackerDealtDamageEvent.Damage = Damage;
			attackerDealtDamageEvent.Object = Object;
			attackerDealtDamageEvent.Actor = Actor;
			attackerDealtDamageEvent.Source = Source;
			attackerDealtDamageEvent.Weapon = Weapon;
			attackerDealtDamageEvent.Projectile = Projectile;
			attackerDealtDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(attackerDealtDamageEvent);
			flag = Actor.HandleEvent(attackerDealtDamageEvent);
			ParentEvent?.ProcessChildEvent(attackerDealtDamageEvent);
		}
	}
}
