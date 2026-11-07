using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class TakenEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(TakenEvent), null, CountPool, ResetPool);

	private static List<TakenEvent> Pool;

	private static int PoolCounter;

	public string Context;

	public TakenEvent()
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

	public static void ResetTo(ref TakenEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static TakenEvent FromPool()
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
		if (flag && GameObject.Validate(ref Item) && Item.HasRegisteredEvent("Taken"))
		{
			Event obj = Event.New("Taken");
			obj.SetParameter("Item", Item);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("TakingObject", Actor);
			obj.SetParameter("Context", Context);
			flag = Item.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			TakenEvent takenEvent = FromPool();
			takenEvent.Item = Item;
			takenEvent.Actor = Actor;
			takenEvent.Context = Context;
			flag = Item.HandleEvent(takenEvent);
		}
	}
}
