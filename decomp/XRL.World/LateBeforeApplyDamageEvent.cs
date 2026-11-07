using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class LateBeforeApplyDamageEvent : IDamageEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(LateBeforeApplyDamageEvent), null, CountPool, ResetPool);

	private static List<LateBeforeApplyDamageEvent> Pool;

	private static int PoolCounter;

	public LateBeforeApplyDamageEvent()
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

	public static void ResetTo(ref LateBeforeApplyDamageEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static LateBeforeApplyDamageEvent FromPool()
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

	public static bool Check(Damage Damage, GameObject Object, GameObject Actor, GameObject Source = null, GameObject Weapon = null, GameObject Projectile = null, bool Indirect = false, Event ParentEvent = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("LateBeforeApplyDamage"))
		{
			Event obj = Event.New("LateBeforeApplyDamage");
			obj.SetParameter("Damage", Damage);
			obj.SetParameter("Object", Object);
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
			LateBeforeApplyDamageEvent lateBeforeApplyDamageEvent = FromPool();
			lateBeforeApplyDamageEvent.Damage = Damage;
			lateBeforeApplyDamageEvent.Object = Object;
			lateBeforeApplyDamageEvent.Actor = Actor;
			lateBeforeApplyDamageEvent.Source = Source;
			lateBeforeApplyDamageEvent.Weapon = Weapon;
			lateBeforeApplyDamageEvent.Projectile = Projectile;
			lateBeforeApplyDamageEvent.Indirect = Indirect;
			ParentEvent?.PreprocessChildEvent(lateBeforeApplyDamageEvent);
			flag = Object.HandleEvent(lateBeforeApplyDamageEvent);
			ParentEvent?.ProcessChildEvent(lateBeforeApplyDamageEvent);
		}
		return flag;
	}
}
