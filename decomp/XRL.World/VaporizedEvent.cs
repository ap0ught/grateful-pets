namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class VaporizedEvent : PooledEvent<VaporizedEvent>
{
	public GameObject Object;

	public GameObject By;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		By = null;
	}

	public static VaporizedEvent FromPool(GameObject Object, GameObject By = null)
	{
		VaporizedEvent vaporizedEvent = PooledEvent<VaporizedEvent>.FromPool();
		vaporizedEvent.Object = Object;
		vaporizedEvent.By = By;
		return vaporizedEvent;
	}

	public static bool Check(GameObject Object, GameObject By = null)
	{
		if (Object.WantEvent(PooledEvent<VaporizedEvent>.ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, By)))
		{
			return false;
		}
		if (Object.HasRegisteredEvent("Vaporized") && !Object.FireEvent(Event.New("Vaporized", "Object", Object, "By", By)))
		{
			return false;
		}
		return true;
	}
}
