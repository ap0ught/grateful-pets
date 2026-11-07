using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 15)]
public class DeathEvent : IDeathEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(DeathEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 15;

	private static List<DeathEvent> Pool;

	private static int PoolCounter;

	public DeathEvent()
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

	public static void ResetTo(ref DeathEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static DeathEvent FromPool()
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
		try
		{
			if (Killer != null)
			{
				DeathEvent deathEvent = FromPool();
				deathEvent.Dying = Dying;
				deathEvent.Killer = Killer;
				deathEvent.Weapon = Weapon;
				deathEvent.Projectile = Projectile;
				deathEvent.Accidental = Accidental;
				deathEvent.AlwaysUsePopups = AlwaysUsePopups;
				deathEvent.KillerText = KillerText;
				deathEvent.Reason = Reason;
				deathEvent.ThirdPersonReason = ThirdPersonReason;
				Killer.HandleEvent(deathEvent);
				Reason = deathEvent.Reason;
				ThirdPersonReason = deathEvent.ThirdPersonReason;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Death MinEvent handling", x);
		}
	}

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, bool AlwaysUsePopups = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		Send(Dying, Killer, ref Reason, ref ThirdPersonReason, Weapon, Projectile, Accidental, AlwaysUsePopups, KillerText);
	}
}
