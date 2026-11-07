namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class SyncRenderEvent : PooledEvent<SyncRenderEvent>
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

	public static SyncRenderEvent FromPool(GameObject Object)
	{
		SyncRenderEvent syncRenderEvent = PooledEvent<SyncRenderEvent>.FromPool();
		syncRenderEvent.Object = Object;
		return syncRenderEvent;
	}

	public static void Send(GameObject Object)
	{
		if (Object.WantEvent(PooledEvent<SyncRenderEvent>.ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object));
		}
	}
}
