using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeDieEvent : IDeathEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeDieEvent), null, CountPool, ResetPool);

	private static List<BeforeDieEvent> Pool;

	private static int PoolCounter;

	public BeforeDieEvent()
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

	public static void ResetTo(ref BeforeDieEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeDieEvent FromPool()
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

	public static bool Check(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, bool AlwaysUsePopups = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.Validate(ref Dying) && Dying.HasRegisteredEvent("BeforeDie"))
			{
				Event obj = Event.New("BeforeDie");
				obj.SetParameter("Dying", Dying);
				obj.SetParameter("Killer", Killer);
				obj.SetParameter("Weapon", Weapon);
				obj.SetParameter("Projectile", Projectile);
				obj.SetParameter("KillerText", KillerText);
				obj.SetParameter("Reason", Reason);
				obj.SetParameter("ThirdPersonReason", ThirdPersonReason);
				obj.SetFlag("Accidental", Accidental);
				obj.SetFlag("AlwaysUsePopups", AlwaysUsePopups);
				flag = Dying.FireEvent(obj);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("BeforeDie registered event handling", x);
		}
		try
		{
			if (flag && GameObject.Validate(ref Dying) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
			{
				BeforeDieEvent beforeDieEvent = FromPool();
				beforeDieEvent.Dying = Dying;
				beforeDieEvent.Killer = Killer;
				beforeDieEvent.Weapon = Weapon;
				beforeDieEvent.Projectile = Projectile;
				beforeDieEvent.Accidental = Accidental;
				beforeDieEvent.AlwaysUsePopups = AlwaysUsePopups;
				beforeDieEvent.KillerText = KillerText;
				beforeDieEvent.Reason = Reason;
				beforeDieEvent.ThirdPersonReason = ThirdPersonReason;
				flag = Dying.HandleEvent(beforeDieEvent);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("BeforeDie MinEvent handling", x2);
		}
		return flag;
	}
}
