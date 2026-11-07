using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetCleaningItemsNearbyEvent : PooledEvent<GetCleaningItemsNearbyEvent>
{
	public GameObject Actor;

	public GameObject Item;

	public List<GameObject> Objects;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Item = null;
		Objects = null;
	}

	public static GetCleaningItemsNearbyEvent FromPool(GameObject Actor, GameObject Item, List<GameObject> Objects)
	{
		GetCleaningItemsNearbyEvent getCleaningItemsNearbyEvent = PooledEvent<GetCleaningItemsNearbyEvent>.FromPool();
		getCleaningItemsNearbyEvent.Actor = Actor;
		getCleaningItemsNearbyEvent.Item = Item;
		getCleaningItemsNearbyEvent.Objects = Objects;
		return getCleaningItemsNearbyEvent;
	}

	public static void Send(GameObject Actor, GameObject Item, ref List<GameObject> Objects)
	{
		Cell cell = Actor?.CurrentCell;
		if (cell == null)
		{
			return;
		}
		GetCleaningItemsNearbyEvent getCleaningItemsNearbyEvent = null;
		if (cell.WantEvent(PooledEvent<GetCleaningItemsNearbyEvent>.ID, MinEvent.CascadeLevel))
		{
			if (getCleaningItemsNearbyEvent == null)
			{
				if (Objects == null)
				{
					Objects = new List<GameObject>();
				}
				getCleaningItemsNearbyEvent = FromPool(Actor, Item, Objects);
			}
			if (!cell.HandleEvent(getCleaningItemsNearbyEvent))
			{
				return;
			}
		}
		foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
		{
			if (!localAdjacentCell.WantEvent(PooledEvent<GetCleaningItemsNearbyEvent>.ID, MinEvent.CascadeLevel))
			{
				continue;
			}
			if (getCleaningItemsNearbyEvent == null)
			{
				if (Objects == null)
				{
					Objects = new List<GameObject>();
				}
				getCleaningItemsNearbyEvent = FromPool(Actor, Item, Objects);
			}
			if (!localAdjacentCell.HandleEvent(getCleaningItemsNearbyEvent))
			{
				break;
			}
		}
	}
}
