using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class KilledEvent : IDeathEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(KilledEvent), null, CountPool, ResetPool);

	private static List<KilledEvent> Pool;

	private static int PoolCounter;

	public KilledEvent()
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

	public static void ResetTo(ref KilledEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static KilledEvent FromPool()
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

	public static void Send(GameObject Dying, GameObject Killer, ref string Reason, ref string ThirdPersonReason, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, bool AlwaysUsePopups = false, string KillerText = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.Validate(ref Killer) && Killer.HasRegisteredEvent("Killed"))
			{
				Event obj = Event.New("Killed");
				obj.SetParameter("Object", Dying);
				obj.SetParameter("Killer", Killer);
				obj.SetParameter("Weapon", Weapon);
				obj.SetParameter("Projectile", Projectile);
				obj.SetParameter("KillerText", KillerText);
				obj.SetParameter("Reason", Reason);
				obj.SetParameter("ThirdPersonReason", ThirdPersonReason);
				obj.SetFlag("Accidental", Accidental);
				obj.SetFlag("AlwaysUsePopups", AlwaysUsePopups);
				flag = Killer.FireEvent(obj);
				Reason = obj.GetStringParameter("Reason");
				ThirdPersonReason = obj.GetStringParameter("ThirdPersonReason");
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Killed registered event handling", x);
		}
		try
		{
			if (flag && GameObject.Validate(ref Killer) && Killer.WantEvent(ID, MinEvent.CascadeLevel))
			{
				KilledEvent killedEvent = FromPool();
				killedEvent.Dying = Dying;
				killedEvent.Killer = Killer;
				killedEvent.Weapon = Weapon;
				killedEvent.Projectile = Projectile;
				killedEvent.Accidental = Accidental;
				killedEvent.AlwaysUsePopups = AlwaysUsePopups;
				killedEvent.KillerText = KillerText;
				killedEvent.Reason = Reason;
				killedEvent.ThirdPersonReason = ThirdPersonReason;
				flag = Killer.HandleEvent(killedEvent);
				Reason = killedEvent.Reason;
				ThirdPersonReason = killedEvent.ThirdPersonReason;
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("Killed MinEvent handling", x2);
		}
	}

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, bool AlwaysUsePopups = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		Send(Dying, Killer, ref Reason, ref ThirdPersonReason, Weapon, Projectile, Accidental, AlwaysUsePopups, KillerText);
	}
}
