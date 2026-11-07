namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class IdleQueryEvent : PooledEvent<IdleQueryEvent>
{
	public GameObject Actor;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
	}

	public static IdleQueryEvent FromPool(GameObject Actor)
	{
		IdleQueryEvent idleQueryEvent = PooledEvent<IdleQueryEvent>.FromPool();
		idleQueryEvent.Actor = Actor;
		return idleQueryEvent;
	}
}
