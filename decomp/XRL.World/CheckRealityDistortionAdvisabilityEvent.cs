namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckRealityDistortionAdvisabilityEvent : PooledEvent<CheckRealityDistortionAdvisabilityEvent>
{
	public GameObject Object;

	public Cell Cell;

	public GameObject Actor;

	public GameObject Device;

	public IPart Mutation;

	public int Threshold;

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
		Threshold = 0;
		Penetration = 0;
	}

	public static bool Check(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int? Threshold = null, int Penetration = 0)
	{
		bool flag = true;
		int num = Threshold ?? ((Mutation != null) ? 80 : 30);
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CheckRealityDistortionAdvisability");
			bool flag3 = Cell?.HasObjectWithRegisteredEvent("CheckRealityDistortionAdvisability") ?? false;
			if (flag2 | flag3)
			{
				Event obj = Event.New("CheckRealityDistortionAdvisability");
				obj.SetParameter("Object", Object);
				obj.SetParameter("Cell", Cell);
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Mutation", Mutation);
				obj.SetParameter("Device", Device);
				obj.SetParameter("Threshold", num);
				obj.SetParameter("Penetration", Penetration);
				flag = (!flag2 || Object.FireEvent(obj)) && (!flag3 || Cell.FireEvent(obj));
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckRealityDistortionAdvisabilityEvent>.ID, MinEvent.CascadeLevel);
			bool flag5 = Cell?.WantEvent(PooledEvent<CheckRealityDistortionAdvisabilityEvent>.ID, MinEvent.CascadeLevel) ?? false;
			if (flag4 | flag5)
			{
				CheckRealityDistortionAdvisabilityEvent checkRealityDistortionAdvisabilityEvent = PooledEvent<CheckRealityDistortionAdvisabilityEvent>.FromPool();
				checkRealityDistortionAdvisabilityEvent.Object = Object;
				checkRealityDistortionAdvisabilityEvent.Cell = Cell;
				checkRealityDistortionAdvisabilityEvent.Actor = Actor;
				checkRealityDistortionAdvisabilityEvent.Mutation = Mutation;
				checkRealityDistortionAdvisabilityEvent.Device = Device;
				checkRealityDistortionAdvisabilityEvent.Threshold = num;
				checkRealityDistortionAdvisabilityEvent.Penetration = Penetration;
				flag = (!flag4 || Object.HandleEvent(checkRealityDistortionAdvisabilityEvent)) && (!flag5 || Cell.HandleEvent(checkRealityDistortionAdvisabilityEvent));
			}
		}
		return flag;
	}
}
