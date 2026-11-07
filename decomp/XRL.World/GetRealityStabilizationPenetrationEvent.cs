namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetRealityStabilizationPenetrationEvent : PooledEvent<GetRealityStabilizationPenetrationEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public int Penetration;

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
		Penetration = 0;
	}

	public static int GetFor(GameObject Object)
	{
		int num = Object.GetIntProperty("RealityStabilizationPenetration");
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetRealityStabilizationPenetration"))
		{
			Event obj = Event.New("GetRealityStabilizationPenetration");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Penetration", num);
			flag = Object.FireEvent(obj);
			num = obj.GetIntParameter("Penetration");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetRealityStabilizationPenetrationEvent>.ID, CascadeLevel))
		{
			GetRealityStabilizationPenetrationEvent getRealityStabilizationPenetrationEvent = PooledEvent<GetRealityStabilizationPenetrationEvent>.FromPool();
			getRealityStabilizationPenetrationEvent.Object = Object;
			getRealityStabilizationPenetrationEvent.Penetration = num;
			flag = Object.HandleEvent(getRealityStabilizationPenetrationEvent);
			num = getRealityStabilizationPenetrationEvent.Penetration;
		}
		return num;
	}
}
