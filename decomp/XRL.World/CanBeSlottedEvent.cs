using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanBeSlottedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanBeSlottedEvent), null, CountPool, ResetPool);

	private static List<CanBeSlottedEvent> Pool;

	private static int PoolCounter;

	public IEnergyCell Cell;

	public CanBeSlottedEvent()
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

	public static void ResetTo(ref CanBeSlottedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CanBeSlottedEvent FromPool()
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
		Cell = null;
	}

	public static bool Check(GameObject Actor, GameObject Item, IEnergyCell Cell)
	{
		bool flag = Actor?.WantEvent(ID, MinEvent.CascadeLevel) ?? false;
		bool flag2 = Item?.WantEvent(ID, MinEvent.CascadeLevel) ?? false;
		if (flag | flag2)
		{
			CanBeSlottedEvent canBeSlottedEvent = FromPool();
			canBeSlottedEvent.Actor = Actor;
			canBeSlottedEvent.Item = Item;
			canBeSlottedEvent.Cell = Cell;
			if (flag && !Actor.HandleEvent(canBeSlottedEvent))
			{
				return false;
			}
			if (flag2 && !Item.HandleEvent(canBeSlottedEvent))
			{
				return false;
			}
		}
		bool flag3 = Actor?.HasRegisteredEvent("CanBeSlotted") ?? false;
		bool flag4 = Item?.HasRegisteredEvent("CanBeSlotted") ?? false;
		if (flag3 | flag4)
		{
			Event e = Event.New("CanBeSlotted", "Actor", Actor, "Item", Item, "Cell", Cell);
			if (flag3 && !Actor.FireEvent(e))
			{
				return false;
			}
			if (flag4 && !Item.FireEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}
