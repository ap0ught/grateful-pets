namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckRealityDistortionUsabilityEvent : PooledEvent<CheckRealityDistortionUsabilityEvent>
{
	public GameObject Object;

	public Cell Cell;

	public GameObject Actor;

	public GameObject Device;

	public IPart Mutation;

	public int? Threshold;

	public int Penetration;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Cell = null;
		Actor = null;
		Device = null;
		Mutation = null;
		Threshold = null;
		Penetration = 0;
	}

	public static bool Check(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int? Threshold = null, int Penetration = 0)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CheckRealityDistortionUsability");
			bool flag3 = Cell?.HasObjectWithRegisteredEvent("CheckRealityDistortionUsability") ?? false;
			if (flag2 | flag3)
			{
				Event obj = Event.New("CheckRealityDistortionUsability");
				obj.SetParameter("Object", Object);
				obj.SetParameter("Cell", Cell);
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Mutation", Mutation);
				obj.SetParameter("Device", Device);
				if (Threshold.HasValue)
				{
					obj.SetParameter("Threshold", Threshold);
				}
				obj.SetParameter("Penetration", Penetration);
				flag = (!flag2 || Object.FireEvent(obj)) && (!flag3 || Cell.FireEvent(obj));
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckRealityDistortionUsabilityEvent>.ID, MinEvent.CascadeLevel);
			bool flag5 = Cell?.WantEvent(PooledEvent<CheckRealityDistortionUsabilityEvent>.ID, MinEvent.CascadeLevel) ?? false;
			if (flag4 | flag5)
			{
				CheckRealityDistortionUsabilityEvent checkRealityDistortionUsabilityEvent = PooledEvent<CheckRealityDistortionUsabilityEvent>.FromPool();
				checkRealityDistortionUsabilityEvent.Object = Object;
				checkRealityDistortionUsabilityEvent.Cell = Cell;
				checkRealityDistortionUsabilityEvent.Actor = Actor;
				checkRealityDistortionUsabilityEvent.Mutation = Mutation;
				checkRealityDistortionUsabilityEvent.Device = Device;
				checkRealityDistortionUsabilityEvent.Threshold = Threshold;
				checkRealityDistortionUsabilityEvent.Penetration = Penetration;
				flag = (!flag4 || Object.HandleEvent(checkRealityDistortionUsabilityEvent)) && (!flag5 || Cell.HandleEvent(checkRealityDistortionUsabilityEvent));
			}
		}
		return flag;
	}
}
