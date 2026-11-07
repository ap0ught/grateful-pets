using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class TookEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(TookEvent), null, CountPool, ResetPool);

	private static List<TookEvent> Pool;

	private static int PoolCounter;

	public string Context;

	public TookEvent()
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

	public static void ResetTo(ref TookEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static TookEvent FromPool()
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
		Context = null;
	}

	public static void Send(GameObject Item, GameObject Actor, string Context = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("Took"))
		{
			Event obj = Event.New("Took");
			obj.SetParameter("Item", Item);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Object", Item);
			obj.SetParameter("Context", Context);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			TookEvent tookEvent = FromPool();
			tookEvent.Item = Item;
			tookEvent.Actor = Actor;
			tookEvent.Context = Context;
			flag = Actor.HandleEvent(tookEvent);
		}
	}
}
