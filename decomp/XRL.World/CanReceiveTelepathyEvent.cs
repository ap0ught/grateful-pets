namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanReceiveTelepathyEvent : PooledEvent<CanReceiveTelepathyEvent>
{
	public GameObject Object;

	public GameObject Actor;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Actor = null;
	}

	public static CanReceiveTelepathyEvent FromPool(GameObject Object, GameObject Actor)
	{
		CanReceiveTelepathyEvent canReceiveTelepathyEvent = PooledEvent<CanReceiveTelepathyEvent>.FromPool();
		canReceiveTelepathyEvent.Object = Object;
		canReceiveTelepathyEvent.Actor = Actor;
		return canReceiveTelepathyEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor)
	{
		if (Object.WantEvent(PooledEvent<CanReceiveTelepathyEvent>.ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Actor)))
		{
			return false;
		}
		return true;
	}
}
