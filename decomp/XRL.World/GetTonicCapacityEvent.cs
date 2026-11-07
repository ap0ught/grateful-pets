namespace XRL.World;

[GameEvent(Cache = Cache.Singleton)]
public class GetTonicCapacityEvent : SingletonEvent<GetTonicCapacityEvent>
{
	public GameObject Actor;

	public int BaseCapacity;

	public int Capacity;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		BaseCapacity = 0;
		Capacity = 0;
	}

	public static int GetFor(GameObject Actor, int BaseCapacity = 1)
	{
		SingletonEvent<GetTonicCapacityEvent>.Instance.Actor = Actor;
		SingletonEvent<GetTonicCapacityEvent>.Instance.BaseCapacity = BaseCapacity;
		SingletonEvent<GetTonicCapacityEvent>.Instance.Capacity = BaseCapacity;
		Actor.HandleEvent(SingletonEvent<GetTonicCapacityEvent>.Instance);
		return SingletonEvent<GetTonicCapacityEvent>.Instance.Capacity;
	}
}
