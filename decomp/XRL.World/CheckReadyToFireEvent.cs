namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckReadyToFireEvent : PooledEvent<CheckReadyToFireEvent>
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
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CheckReadyToFire"))
		{
			Event obj = Event.New("CheckReadyToFire");
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckReadyToFireEvent>.ID, MinEvent.CascadeLevel))
		{
			CheckReadyToFireEvent checkReadyToFireEvent = PooledEvent<CheckReadyToFireEvent>.FromPool();
			checkReadyToFireEvent.Object = Object;
			flag = Object.HandleEvent(checkReadyToFireEvent);
		}
		return flag;
	}
}
