using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterThrownEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterThrownEvent), null, CountPool, ResetPool);

	private static List<AfterThrownEvent> Pool;

	private static int PoolCounter;

	public GameObject ApparentTarget;

	public AfterThrownEvent()
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

	public static void ResetTo(ref AfterThrownEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterThrownEvent FromPool()
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

	public static AfterThrownEvent FromPool(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		AfterThrownEvent afterThrownEvent = FromPool();
		afterThrownEvent.Actor = Actor;
		afterThrownEvent.Item = Item;
		afterThrownEvent.ApparentTarget = ApparentTarget;
		return afterThrownEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		if (GameObject.Validate(ref Item) && Item.HasRegisteredEvent("AfterThrown"))
		{
			Event obj = Event.New("AfterThrown");
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
			AfterThrownEvent e = FromPool(Actor, Item, ApparentTarget);
			if (!Item.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}
