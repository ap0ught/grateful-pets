using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class KilledPlayerEvent : IDeathEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(KilledPlayerEvent), null, CountPool, ResetPool);

	private static List<KilledPlayerEvent> Pool;

	private static int PoolCounter;

	public KilledPlayerEvent()
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

	public static void ResetTo(ref KilledPlayerEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static KilledPlayerEvent FromPool()
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
			if (flag && GameObject.Validate(ref Killer) && Dying.HasRegisteredEvent("KilledPlayer"))
			{
				Event obj = Event.New("KilledPlayer");
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
			MetricsManager.LogError("KilledPlayer registered event handling", x);
		}
		try
		{
			if (flag && GameObject.Validate(ref Killer) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
			{
				KilledPlayerEvent killedPlayerEvent = FromPool();
				killedPlayerEvent.Dying = Dying;
				killedPlayerEvent.Killer = Killer;
				killedPlayerEvent.Weapon = Weapon;
				killedPlayerEvent.Projectile = Projectile;
				killedPlayerEvent.Accidental = Accidental;
				killedPlayerEvent.AlwaysUsePopups = AlwaysUsePopups;
				killedPlayerEvent.KillerText = KillerText;
				killedPlayerEvent.Reason = Reason;
				killedPlayerEvent.ThirdPersonReason = ThirdPersonReason;
				flag = Killer.HandleEvent(killedPlayerEvent);
				Reason = killedPlayerEvent.Reason;
				ThirdPersonReason = killedPlayerEvent.ThirdPersonReason;
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("KilledPlayer MinEvent handling", x2);
		}
	}

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, bool AlwaysUsePopups = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		Send(Dying, Killer, ref Reason, ref ThirdPersonReason, Weapon, Projectile, Accidental, AlwaysUsePopups, KillerText);
	}
}
