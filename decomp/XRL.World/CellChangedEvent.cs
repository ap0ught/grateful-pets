using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CellChangedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CellChangedEvent), null, CountPool, ResetPool);

	private static List<CellChangedEvent> Pool;

	private static int PoolCounter;

	public GameObject OldCell;

	public GameObject NewCell;

	public CellChangedEvent()
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

	public static void ResetTo(ref CellChangedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CellChangedEvent FromPool()
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
		OldCell = null;
		NewCell = null;
	}

	public static CellChangedEvent FromPool(GameObject Actor, GameObject Item, GameObject OldCell, GameObject NewCell)
	{
		CellChangedEvent cellChangedEvent = FromPool();
		cellChangedEvent.Actor = Actor;
		cellChangedEvent.Item = Item;
		cellChangedEvent.OldCell = OldCell;
		cellChangedEvent.NewCell = NewCell;
		return cellChangedEvent;
	}

	public static void Send(GameObject Actor, GameObject Item, GameObject OldCell, GameObject NewCell)
	{
		if (Item.HasRegisteredEvent("CellChanged"))
		{
			Event obj = Event.New("CellChanged");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Item", Item);
			obj.SetParameter("OldCell", OldCell);
			obj.SetParameter("NewCell", NewCell);
			if (!Item.FireEvent(obj))
			{
				return;
			}
		}
		if (Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Item.HandleEvent(FromPool(Actor, Item, OldCell, NewCell));
		}
	}
}
