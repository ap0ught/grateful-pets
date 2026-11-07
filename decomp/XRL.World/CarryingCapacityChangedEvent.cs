namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CarryingCapacityChangedEvent : PooledEvent<CarryingCapacityChangedEvent>
{
	public GameObject Object;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
	}

	public static CarryingCapacityChangedEvent FromPool(GameObject Object)
	{
		CarryingCapacityChangedEvent carryingCapacityChangedEvent = PooledEvent<CarryingCapacityChangedEvent>.FromPool();
		carryingCapacityChangedEvent.Object = Object;
		return carryingCapacityChangedEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.WantEvent(PooledEvent<CarryingCapacityChangedEvent>.ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}
