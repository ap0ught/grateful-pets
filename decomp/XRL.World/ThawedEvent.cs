namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ThawedEvent : PooledEvent<ThawedEvent>
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

	public static void Send(GameObject Object, GameObject By)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("Thawed"))
		{
			Event obj = Event.New("Thawed");
			obj.SetParameter("Object", Object);
			obj.SetParameter("By", By);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<ThawedEvent>.ID, MinEvent.CascadeLevel))
		{
			ThawedEvent thawedEvent = PooledEvent<ThawedEvent>.FromPool();
			thawedEvent.Object = Object;
			thawedEvent.By = By;
			flag = Object.HandleEvent(thawedEvent);
		}
	}
}
