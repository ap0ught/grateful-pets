namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class IsRootedInPlaceEvent : PooledEvent<IsRootedInPlaceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
	}

	public static IsRootedInPlaceEvent FromPool(GameObject Object)
	{
		IsRootedInPlaceEvent isRootedInPlaceEvent = PooledEvent<IsRootedInPlaceEvent>.FromPool();
		isRootedInPlaceEvent.Object = Object;
		return isRootedInPlaceEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object.HasRegisteredEvent("IsRootedInPlace"))
		{
			Event obj = Event.New("IsRootedInPlace");
			obj.SetParameter("Object", Object);
			if (!Object.FireEvent(obj))
			{
				return true;
			}
		}
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<IsRootedInPlaceEvent>.ID, CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return true;
		}
		return false;
	}
}
