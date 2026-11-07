using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AllowInventoryStackEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AllowInventoryStackEvent), null, CountPool, ResetPool);

	private static List<AllowInventoryStackEvent> Pool;

	private static int PoolCounter;

	public AllowInventoryStackEvent()
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

	public static void ResetTo(ref AllowInventoryStackEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AllowInventoryStackEvent FromPool()
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

	public static AllowInventoryStackEvent FromPool(GameObject Actor, GameObject Item)
	{
		AllowInventoryStackEvent allowInventoryStackEvent = FromPool();
		allowInventoryStackEvent.Actor = Actor;
		allowInventoryStackEvent.Item = Item;
		return allowInventoryStackEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		if (GameObject.Validate(ref Actor) && GameObject.Validate(ref Item) && Actor.HasRegisteredEvent("AllowInventoryStack"))
		{
			Event obj = Event.New("AllowInventoryStack");
			obj.SetParameter("Object", Item);
			if (!Actor.FireEvent(obj))
			{
				return false;
			}
		}
		if (GameObject.Validate(ref Actor) && GameObject.Validate(ref Item) && Actor.WantEvent(ID, MinEvent.CascadeLevel) && !Actor.HandleEvent(FromPool(Actor, Item)))
		{
			return false;
		}
		return true;
	}
}
