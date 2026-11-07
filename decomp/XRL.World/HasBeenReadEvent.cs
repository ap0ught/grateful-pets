namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class HasBeenReadEvent : PooledEvent<HasBeenReadEvent>
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

	public static bool Check(GameObject Object, GameObject Actor)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("HasBeenRead"))
		{
			Event obj = Event.New("HasBeenRead");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<HasBeenReadEvent>.ID, MinEvent.CascadeLevel))
		{
			HasBeenReadEvent hasBeenReadEvent = PooledEvent<HasBeenReadEvent>.FromPool();
			hasBeenReadEvent.Object = Object;
			hasBeenReadEvent.Actor = Actor;
			flag = Object.HandleEvent(hasBeenReadEvent);
		}
		return !flag;
	}
}
