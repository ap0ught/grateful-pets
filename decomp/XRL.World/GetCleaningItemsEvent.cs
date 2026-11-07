using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 7, Cache = Cache.Pool)]
public class GetCleaningItemsEvent : PooledEvent<GetCleaningItemsEvent>
{
	public new static readonly int CascadeLevel = 7;

	public GameObject Actor;

	public GameObject Item;

	public List<GameObject> Objects = new List<GameObject>();

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Item = null;
		Objects.Clear();
	}

	public static GetCleaningItemsEvent FromPool(GameObject Actor, GameObject Item)
	{
		GetCleaningItemsEvent getCleaningItemsEvent = PooledEvent<GetCleaningItemsEvent>.FromPool();
		getCleaningItemsEvent.Actor = Actor;
		getCleaningItemsEvent.Item = Item;
		getCleaningItemsEvent.Objects.Clear();
		return getCleaningItemsEvent;
	}

	public static List<GameObject> GetFor(GameObject Actor, GameObject Item)
	{
		GetCleaningItemsEvent getCleaningItemsEvent = null;
		List<GameObject> Objects = null;
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetCleaningItemsEvent>.ID, CascadeLevel))
		{
			if (getCleaningItemsEvent == null)
			{
				getCleaningItemsEvent = FromPool(Actor, Item);
				Objects = getCleaningItemsEvent.Objects;
			}
			Actor.HandleEvent(getCleaningItemsEvent);
		}
		GetCleaningItemsNearbyEvent.Send(Actor, Item, ref Objects);
		return Objects;
	}
}
