using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterAfterThrownEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterAfterThrownEvent), null, CountPool, ResetPool);

	private static List<AfterAfterThrownEvent> Pool;

	private static int PoolCounter;

	public GameObject ApparentTarget;

	public AfterAfterThrownEvent()
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

	public static void ResetTo(ref AfterAfterThrownEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterAfterThrownEvent FromPool()
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

	public override void Reset()
	{
		base.Reset();
		ApparentTarget = null;
	}

	public static AfterAfterThrownEvent FromPool(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		AfterAfterThrownEvent afterAfterThrownEvent = FromPool();
		afterAfterThrownEvent.Actor = Actor;
		afterAfterThrownEvent.Item = Item;
		afterAfterThrownEvent.ApparentTarget = ApparentTarget;
		return afterAfterThrownEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		if (GameObject.Validate(ref Item) && Item.HasRegisteredEvent("AfterAfterThrown"))
		{
			Event obj = Event.New("AfterAfterThrown");
			obj.SetParameter("Owner", Actor);
			obj.SetParameter("Object", Item);
			obj.SetParameter("ApparentTarget", ApparentTarget);
			if (!Item.FireEvent(obj))
			{
				return false;
			}
		}
		if (GameObject.Validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AfterAfterThrownEvent e = FromPool(Actor, Item, ApparentTarget);
			if (!Item.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}
