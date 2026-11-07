namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class CanTemperatureReturnToAmbientEvent : PooledEvent<CanTemperatureReturnToAmbientEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public int Amount;

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
		Amount = 0;
	}

	public static bool Check(GameObject Object, int Amount)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanTemperatureReturnToAmbient"))
		{
			Event obj = Event.New("CanTemperatureReturnToAmbient");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Amount", Amount);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanTemperatureReturnToAmbientEvent>.ID, CascadeLevel))
		{
			CanTemperatureReturnToAmbientEvent canTemperatureReturnToAmbientEvent = PooledEvent<CanTemperatureReturnToAmbientEvent>.FromPool();
			canTemperatureReturnToAmbientEvent.Object = Object;
			canTemperatureReturnToAmbientEvent.Amount = Amount;
			flag = Object.HandleEvent(canTemperatureReturnToAmbientEvent);
		}
		return flag;
	}
}
