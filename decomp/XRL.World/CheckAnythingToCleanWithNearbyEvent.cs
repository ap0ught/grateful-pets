namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckAnythingToCleanWithNearbyEvent : PooledEvent<CheckAnythingToCleanWithNearbyEvent>
{
	public GameObject Actor;

	public GameObject Item;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Item = null;
	}

	public static CheckAnythingToCleanWithNearbyEvent FromPool(GameObject Actor, GameObject Item)
	{
		CheckAnythingToCleanWithNearbyEvent checkAnythingToCleanWithNearbyEvent = PooledEvent<CheckAnythingToCleanWithNearbyEvent>.FromPool();
		checkAnythingToCleanWithNearbyEvent.Actor = Actor;
		checkAnythingToCleanWithNearbyEvent.Item = Item;
		return checkAnythingToCleanWithNearbyEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		Cell cell = Actor?.CurrentCell;
		if (cell != null)
		{
			CheckAnythingToCleanWithNearbyEvent checkAnythingToCleanWithNearbyEvent = null;
			if (cell.WantEvent(PooledEvent<CheckAnythingToCleanWithNearbyEvent>.ID, MinEvent.CascadeLevel))
			{
				if (checkAnythingToCleanWithNearbyEvent == null)
				{
					checkAnythingToCleanWithNearbyEvent = FromPool(Actor, Item);
				}
				if (!cell.HandleEvent(checkAnythingToCleanWithNearbyEvent))
				{
					return true;
				}
			}
			foreach (Cell localAdjacentCell in cell.GetLocalAdjacentCells())
			{
				if (localAdjacentCell.WantEvent(PooledEvent<CheckAnythingToCleanWithNearbyEvent>.ID, MinEvent.CascadeLevel))
				{
					if (checkAnythingToCleanWithNearbyEvent == null)
					{
						checkAnythingToCleanWithNearbyEvent = FromPool(Actor, Item);
					}
					if (!localAdjacentCell.HandleEvent(checkAnythingToCleanWithNearbyEvent))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
