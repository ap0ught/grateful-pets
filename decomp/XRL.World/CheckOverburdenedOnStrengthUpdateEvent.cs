namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckOverburdenedOnStrengthUpdateEvent : PooledEvent<CheckOverburdenedOnStrengthUpdateEvent>
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

	public static CheckOverburdenedOnStrengthUpdateEvent FromPool(GameObject Object)
	{
		CheckOverburdenedOnStrengthUpdateEvent checkOverburdenedOnStrengthUpdateEvent = PooledEvent<CheckOverburdenedOnStrengthUpdateEvent>.FromPool();
		checkOverburdenedOnStrengthUpdateEvent.Object = Object;
		return checkOverburdenedOnStrengthUpdateEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CheckOverburdenedOnStrengthUpdate"))
		{
			Event obj = Event.New("CheckOverburdenedOnStrengthUpdate");
			obj.SetParameter("Object", Object);
			if (!Object.FireEvent(obj))
			{
				return false;
			}
		}
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckOverburdenedOnStrengthUpdateEvent>.ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return false;
		}
		return true;
	}
}
