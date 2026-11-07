namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetFixedMissileSpreadEvent : PooledEvent<GetFixedMissileSpreadEvent>
{
	public GameObject Object;

	public int Spread;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Spread = 0;
	}

	public static bool GetFor(GameObject Object, out int Spread)
	{
		Spread = 0;
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetFixedMissileSpread"))
		{
			Event obj = Event.New("GetFixedMissileSpread");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Spread", Spread);
			flag = Object.FireEvent(obj);
			Spread = obj.GetIntParameter("Spread");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetFixedMissileSpreadEvent>.ID, MinEvent.CascadeLevel))
		{
			GetFixedMissileSpreadEvent getFixedMissileSpreadEvent = PooledEvent<GetFixedMissileSpreadEvent>.FromPool();
			getFixedMissileSpreadEvent.Object = Object;
			getFixedMissileSpreadEvent.Spread = Spread;
			flag = Object.HandleEvent(getFixedMissileSpreadEvent);
			Spread = getFixedMissileSpreadEvent.Spread;
		}
		return !flag;
	}
}
