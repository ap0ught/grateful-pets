namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanReceiveEmpathyEvent : PooledEvent<CanReceiveEmpathyEvent>
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

	public static CanReceiveEmpathyEvent FromPool(GameObject Object, GameObject Actor)
	{
		CanReceiveEmpathyEvent canReceiveEmpathyEvent = PooledEvent<CanReceiveEmpathyEvent>.FromPool();
		canReceiveEmpathyEvent.Object = Object;
		canReceiveEmpathyEvent.Actor = Actor;
		return canReceiveEmpathyEvent;
	}

	public static bool Check(GameObject Object, GameObject Actor)
	{
		if (Object.WantEvent(PooledEvent<CanReceiveEmpathyEvent>.ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Actor)))
		{
			return false;
		}
		return true;
	}
}
