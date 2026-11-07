using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeUnequippedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeUnequippedEvent), null, CountPool, ResetPool);

	private static List<BeforeUnequippedEvent> Pool;

	private static int PoolCounter;

	public BeforeUnequippedEvent()
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

	public static void ResetTo(ref BeforeUnequippedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeUnequippedEvent FromPool()
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

	public static BeforeUnequippedEvent FromPool(GameObject Actor, GameObject Item)
	{
		BeforeUnequippedEvent beforeUnequippedEvent = FromPool();
		beforeUnequippedEvent.Actor = Actor;
		beforeUnequippedEvent.Item = Item;
		return beforeUnequippedEvent;
	}

	public static void Send(GameObject Object, GameObject WasEquippedBy)
	{
		if (!GameObject.Validate(ref Object))
		{
			return;
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(WasEquippedBy, Object));
			if (!GameObject.Validate(ref Object))
			{
				return;
			}
		}
		if (Object.HasRegisteredEvent("BeforeUnequipped"))
		{
			Object.FireEvent(Event.New("BeforeUnequipped", "Actor", WasEquippedBy));
		}
	}
}
