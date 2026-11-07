using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeDestroyObjectEvent : IDestroyObjectEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeDestroyObjectEvent), null, CountPool, ResetPool);

	private static List<BeforeDestroyObjectEvent> Pool;

	private static int PoolCounter;

	public BeforeDestroyObjectEvent()
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

	public static void ResetTo(ref BeforeDestroyObjectEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeDestroyObjectEvent FromPool()
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

	public static BeforeDestroyObjectEvent FromPool(GameObject Object, bool Obliterate = false, bool Silent = false, string Reason = null, string ThirdPersonReason = null)
	{
		BeforeDestroyObjectEvent beforeDestroyObjectEvent = FromPool();
		beforeDestroyObjectEvent.Object = Object;
		beforeDestroyObjectEvent.Obliterate = Obliterate;
		beforeDestroyObjectEvent.Silent = Silent;
		beforeDestroyObjectEvent.Reason = Reason;
		beforeDestroyObjectEvent.ThirdPersonReason = ThirdPersonReason;
		return beforeDestroyObjectEvent;
	}

	public static bool Check(GameObject Object, bool Obliterate = false, bool Silent = false, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeforeDestroyObject"))
			{
				Event obj = Event.New("BeforeDestroyObject");
				obj.SetParameter("Object", Object);
				obj.SetFlag("Obliterate", Obliterate);
				obj.SetSilent(Silent);
				obj.SetParameter("Reason", Reason);
				obj.SetParameter("ThirdPersonReason", ThirdPersonReason);
				flag = Object.FireEvent(obj);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("BeforeDestroyObject registered event handling", x);
		}
		try
		{
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
			{
				BeforeDestroyObjectEvent e = FromPool(Object, Obliterate, Silent, Reason, ThirdPersonReason);
				flag = Object.HandleEvent(e);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("BeforeDestroyObject MinEvent handling", x2);
		}
		return flag;
	}
}
