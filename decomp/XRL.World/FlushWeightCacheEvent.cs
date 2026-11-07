namespace XRL.World;

[GameEvent(Cache = Cache.Singleton)]
public class FlushWeightCacheEvent : SingletonEvent<FlushWeightCacheEvent>
{
	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public static void Send(GameObject Object)
	{
		if (GameObject.Validate(Object) && Object.WantEvent(SingletonEvent<FlushWeightCacheEvent>.ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(SingletonEvent<FlushWeightCacheEvent>.Instance);
		}
	}
}
