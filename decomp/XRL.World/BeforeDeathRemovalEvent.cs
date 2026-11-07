using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeDeathRemovalEvent : IDeathEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeDeathRemovalEvent), null, CountPool, ResetPool);

	private static List<BeforeDeathRemovalEvent> Pool;

	private static int PoolCounter;

	public BeforeDeathRemovalEvent()
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

	public static void ResetTo(ref BeforeDeathRemovalEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeDeathRemovalEvent FromPool()
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

	public static void Send(GameObject Dying, GameObject Killer, GameObject Weapon = null, GameObject Projectile = null, bool Accidental = false, bool AlwaysUsePopups = false, string KillerText = null, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.Validate(ref Dying) && Dying.HasRegisteredEvent("BeforeDeathRemoval"))
			{
				Event obj = Event.New("BeforeDeathRemoval");
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
			MetricsManager.LogError("BeforeDeathRemoval registered event handling", x);
		}
		try
		{
			if (flag && GameObject.Validate(ref Dying) && Dying.WantEvent(ID, MinEvent.CascadeLevel))
			{
				BeforeDeathRemovalEvent beforeDeathRemovalEvent = FromPool();
				beforeDeathRemovalEvent.Dying = Dying;
				beforeDeathRemovalEvent.Killer = Killer;
				beforeDeathRemovalEvent.Weapon = Weapon;
				beforeDeathRemovalEvent.Projectile = Projectile;
				beforeDeathRemovalEvent.Accidental = Accidental;
				beforeDeathRemovalEvent.AlwaysUsePopups = AlwaysUsePopups;
				beforeDeathRemovalEvent.KillerText = KillerText;
				beforeDeathRemovalEvent.Reason = Reason;
				beforeDeathRemovalEvent.ThirdPersonReason = ThirdPersonReason;
				flag = Dying.HandleEvent(beforeDeathRemovalEvent);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("BeforeDeathRemoval MinEvent handling", x2);
		}
	}
}
