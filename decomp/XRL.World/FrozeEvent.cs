namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class FrozeEvent : PooledEvent<FrozeEvent>
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

	public static bool Check(GameObject Object, GameObject By)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("Froze"))
		{
			Event obj = Event.New("Froze");
			obj.SetParameter("Object", Object);
			obj.SetParameter("By", By);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<FrozeEvent>.ID, MinEvent.CascadeLevel))
		{
			FrozeEvent frozeEvent = PooledEvent<FrozeEvent>.FromPool();
			frozeEvent.Object = Object;
			frozeEvent.By = By;
			flag = Object.HandleEvent(frozeEvent);
		}
		return flag;
	}
}
