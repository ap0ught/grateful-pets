using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeTookDamageEvent : IDamageEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeTookDamageEvent), null, CountPool, ResetPool);

	private static List<BeforeTookDamageEvent> Pool;

	private static int PoolCounter;

	public BeforeTookDamageEvent()
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

	public static void ResetTo(ref BeforeTookDamageEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeTookDamageEvent FromPool()
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
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeforeTookDamage"))
		{
			Event obj = Event.New("BeforeTookDamage");
			obj.SetParameter("Damage", Damage);
			obj.SetParameter("Defender", Object);
			obj.SetParameter("Owner", Actor);
			obj.SetParameter("Source", Source);
			obj.SetParameter("Weapon", Weapon);
			obj.SetParameter("Projectile", Projectile);
			obj.SetFlag("Indirect", Indirect);
			ParentEvent?.PreprocessChildEvent(obj);
			flag = Object.FireEvent(obj, ParentEvent);
			ParentEvent?.ProcessChildEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			BeforeTookDamageEvent beforeTookDamageEvent = FromPool();
			beforeTookDamageEvent.Damage = Damage;
			beforeTookDamageEvent.Object = Object;
			beforeTookDamageEvent.Actor = Actor;
			beforeTookDamageEvent.Source = Source;
			beforeTookDamageEvent.Weapon = Weapon;
			beforeTookDamageEvent.Projectile = Projectile;
			beforeTookDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(beforeTookDamageEvent);
			flag = Object.HandleEvent(beforeTookDamageEvent);
			ParentEvent?.ProcessChildEvent(beforeTookDamageEvent);
		}
	}
}
