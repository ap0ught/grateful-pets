using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class DroppedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(DroppedEvent), null, CountPool, ResetPool);

	private static List<DroppedEvent> Pool;

	private static int PoolCounter;

	public bool Forced;

	public DroppedEvent()
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

	public static void ResetTo(ref DroppedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static DroppedEvent FromPool()
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
		Forced = false;
	}

	public static DroppedEvent FromPool(GameObject Actor, GameObject Item, bool Forced = false)
	{
		DroppedEvent droppedEvent = FromPool();
		droppedEvent.Actor = Actor;
		droppedEvent.Item = Item;
		droppedEvent.Forced = Forced;
		return droppedEvent;
	}

	public static void Send(GameObject Actor, GameObject Item, bool Forced = false)
	{
		Item.HandleEvent(FromPool(Actor, Item, Forced));
	}
}
