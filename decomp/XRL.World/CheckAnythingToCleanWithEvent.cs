namespace XRL.World;

[GameEvent(Cascade = 7, Cache = Cache.Pool)]
public class CheckAnythingToCleanWithEvent : PooledEvent<CheckAnythingToCleanWithEvent>
{
	public new static readonly int CascadeLevel = 7;

	public GameObject Actor;

	public GameObject Item;

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
	}

	public static CheckAnythingToCleanWithEvent FromPool(GameObject Actor, GameObject Item)
	{
		CheckAnythingToCleanWithEvent checkAnythingToCleanWithEvent = PooledEvent<CheckAnythingToCleanWithEvent>.FromPool();
		checkAnythingToCleanWithEvent.Actor = Actor;
		checkAnythingToCleanWithEvent.Item = Item;
		return checkAnythingToCleanWithEvent;
	}

	public static bool Check(GameObject Actor, GameObject Item)
	{
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<CheckAnythingToCleanWithEvent>.ID, CascadeLevel) && !Actor.HandleEvent(FromPool(Actor, Item)))
		{
			return true;
		}
		if (CheckAnythingToCleanWithNearbyEvent.Check(Actor, Item))
		{
			return true;
		}
		return false;
	}
}
