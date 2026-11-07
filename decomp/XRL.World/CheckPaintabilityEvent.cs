namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckPaintabilityEvent : PooledEvent<CheckPaintabilityEvent>
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

	public static CheckPaintabilityEvent FromPool(GameObject Object)
	{
		CheckPaintabilityEvent checkPaintabilityEvent = PooledEvent<CheckPaintabilityEvent>.FromPool();
		checkPaintabilityEvent.Object = Object;
		return checkPaintabilityEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckPaintabilityEvent>.ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return false;
		}
		return true;
	}
}
