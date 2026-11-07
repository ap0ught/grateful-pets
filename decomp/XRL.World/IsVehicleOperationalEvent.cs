namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class IsVehicleOperationalEvent : PooledEvent<IsVehicleOperationalEvent>
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

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (GameObject.Validate(ref Object))
		{
			if (Object.HasRegisteredEvent("IsVehicleOperational"))
			{
				Event obj = Event.New("IsVehicleOperational");
				obj.SetParameter("Object", Object);
				flag = Object.FireEvent(obj);
			}
			if (flag && Object.WantEvent(PooledEvent<IsVehicleOperationalEvent>.ID, CascadeLevel))
			{
				IsVehicleOperationalEvent isVehicleOperationalEvent = PooledEvent<IsVehicleOperationalEvent>.FromPool();
				isVehicleOperationalEvent.Object = Object;
				flag = Object.HandleEvent(isVehicleOperationalEvent);
			}
		}
		return flag;
	}
}
