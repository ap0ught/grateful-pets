using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeAfterThrownEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeAfterThrownEvent), null, CountPool, ResetPool);

	private static List<BeforeAfterThrownEvent> Pool;

	private static int PoolCounter;

	public GameObject ApparentTarget;

	public BeforeAfterThrownEvent()
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

	public static void ResetTo(ref BeforeAfterThrownEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeAfterThrownEvent FromPool()
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

	public static BeforeAfterThrownEvent FromPool(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		BeforeAfterThrownEvent beforeAfterThrownEvent = FromPool();
		beforeAfterThrownEvent.Actor = Actor;
		beforeAfterThrownEvent.Item = Item;
		beforeAfterThrownEvent.ApparentTarget = ApparentTarget;
		return beforeAfterThrownEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item, GameObject ApparentTarget)
	{
		if (GameObject.Validate(ref Item) && Item.HasRegisteredEvent("BeforeAfterThrown"))
		{
			Event obj = Event.New("BeforeAfterThrown");
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
			BeforeAfterThrownEvent e = FromPool(Actor, Item, ApparentTarget);
			if (!Item.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}
