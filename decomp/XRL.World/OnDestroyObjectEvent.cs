using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class OnDestroyObjectEvent : IDestroyObjectEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(OnDestroyObjectEvent), null, CountPool, ResetPool);

	private static List<OnDestroyObjectEvent> Pool;

	private static int PoolCounter;

	public OnDestroyObjectEvent()
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

	public static void ResetTo(ref OnDestroyObjectEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static OnDestroyObjectEvent FromPool()
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

	public static OnDestroyObjectEvent FromPool(GameObject Object, bool Obliterate = false, bool Silent = false, string Reason = null, string ThirdPersonReason = null)
	{
		OnDestroyObjectEvent onDestroyObjectEvent = FromPool();
		onDestroyObjectEvent.Object = Object;
		onDestroyObjectEvent.Obliterate = Obliterate;
		onDestroyObjectEvent.Silent = Silent;
		onDestroyObjectEvent.Reason = Reason;
		onDestroyObjectEvent.ThirdPersonReason = ThirdPersonReason;
		return onDestroyObjectEvent;
	}

	public static void Send(GameObject Object, bool Obliterate = false, bool Silent = false, string Reason = null, string ThirdPersonReason = null)
	{
		bool flag = true;
		try
		{
			if (flag && GameObject.Validate(Object) && Object.HasRegisteredEvent("OnDestroyObject"))
			{
				Event obj = Event.New("OnDestroyObject");
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
			MetricsManager.LogError("OnDestroyObject registered event handling", x);
		}
		try
		{
			if (flag && GameObject.Validate(Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
			{
				OnDestroyObjectEvent e = FromPool(Object, Obliterate, Silent, Reason, ThirdPersonReason);
				flag = Object.HandleEvent(e);
			}
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("OnDestroyObject MinEvent handling", x2);
		}
	}
}
