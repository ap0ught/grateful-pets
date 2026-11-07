namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class IsExplosiveEvent : PooledEvent<IsExplosiveEvent>
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
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("IsExplosive"))
		{
			Event obj = Event.New("IsExplosive");
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<IsExplosiveEvent>.ID, MinEvent.CascadeLevel))
		{
			IsExplosiveEvent isExplosiveEvent = PooledEvent<IsExplosiveEvent>.FromPool();
			isExplosiveEvent.Object = Object;
			flag = Object.HandleEvent(isExplosiveEvent);
		}
		return !flag;
	}
}
