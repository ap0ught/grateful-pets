namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetInventoryCategoryEvent : PooledEvent<GetInventoryCategoryEvent>
{
	public GameObject Object;

	public string Category = "";

	public bool AsIfKnown;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Category = "";
		AsIfKnown = false;
	}

	public static GetInventoryCategoryEvent FromPool(GameObject Object, string Category = "")
	{
		GetInventoryCategoryEvent getInventoryCategoryEvent = PooledEvent<GetInventoryCategoryEvent>.FromPool();
		getInventoryCategoryEvent.Object = Object;
		getInventoryCategoryEvent.Category = Category;
		getInventoryCategoryEvent.AsIfKnown = false;
		return getInventoryCategoryEvent;
	}
}
