using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class CanBeNamedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanBeNamedEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 1;

	private static List<CanBeNamedEvent> Pool;

	private static int PoolCounter;

	public CanBeNamedEvent()
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

	public static void ResetTo(ref CanBeNamedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CanBeNamedEvent FromPool()
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

	public static CanBeNamedEvent FromPool(GameObject Actor, GameObject Item)
	{
		CanBeNamedEvent canBeNamedEvent = FromPool();
		canBeNamedEvent.Actor = Actor;
		canBeNamedEvent.Item = Item;
		return canBeNamedEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		bool flag = Actor?.WantEvent(ID, CascadeLevel) ?? false;
		bool flag2 = Item?.WantEvent(ID, CascadeLevel) ?? false;
		if (flag | flag2)
		{
			CanBeNamedEvent e = FromPool(Actor, Item);
			if (flag && !Actor.HandleEvent(e))
			{
				return false;
			}
			if (flag2 && !Item.HandleEvent(e))
			{
				return false;
			}
		}
		bool flag3 = Actor?.HasRegisteredEvent("CanBeNamed") ?? false;
		bool flag4 = Item?.HasRegisteredEvent("CanBeNamed") ?? false;
		if (flag3 | flag4)
		{
			Event e2 = Event.New("CanBeNamed", "Actor", Actor, "Item", Item);
			if (flag3 && !Actor.FireEvent(e2))
			{
				return false;
			}
			if (flag4 && !Item.FireEvent(e2))
			{
				return false;
			}
		}
		return true;
	}
}
