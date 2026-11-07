using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class UnequippedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(UnequippedEvent), null, CountPool, ResetPool);

	private static List<UnequippedEvent> Pool;

	private static int PoolCounter;

	public UnequippedEvent()
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

	public static void ResetTo(ref UnequippedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static UnequippedEvent FromPool()
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

	public static void Send(GameObject Object, GameObject WasEquippedBy)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("Unequipped"))
		{
			Event obj = new Event("Unequipped");
			obj.SetParameter("UnequippingObject", WasEquippedBy);
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", WasEquippedBy);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			UnequippedEvent unequippedEvent = FromPool();
			unequippedEvent.Actor = WasEquippedBy;
			unequippedEvent.Item = Object;
			flag = Object.HandleEvent(unequippedEvent);
		}
	}
}
