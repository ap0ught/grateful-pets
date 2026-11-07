namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class IsAfflictionEvent : PooledEvent<IsAfflictionEvent>
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

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("IsAffliction"))
		{
			Event obj = Event.New("IsAffliction");
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<IsAfflictionEvent>.ID, MinEvent.CascadeLevel))
		{
			IsAfflictionEvent isAfflictionEvent = PooledEvent<IsAfflictionEvent>.FromPool();
			isAfflictionEvent.Object = Object;
			flag = Object.HandleEvent(isAfflictionEvent);
		}
		return !flag;
	}
}
