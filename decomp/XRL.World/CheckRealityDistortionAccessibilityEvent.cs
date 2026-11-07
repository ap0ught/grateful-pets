namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckRealityDistortionAccessibilityEvent : PooledEvent<CheckRealityDistortionAccessibilityEvent>
{
	public GameObject Object;

	public Cell Cell;

	public GameObject Actor;

	public GameObject Device;

	public IPart Mutation;

	public int Penetration;

	public bool Allow;

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
		Penetration = 0;
		Allow = false;
	}

	public static bool Check(GameObject Object = null, Cell Cell = null, GameObject Actor = null, GameObject Device = null, IPart Mutation = null, int Penetration = 0)
	{
		bool flag = true;
		bool flag2 = true;
		if (Cell == null)
		{
			Cell = Object?.CurrentCell;
		}
		if (flag)
		{
			bool flag3 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CheckRealityDistortionAccessibility");
			bool flag4 = Cell?.HasObjectWithRegisteredEvent("CheckRealityDistortionAccessibility") ?? false;
			if (flag3 | flag4)
			{
				Event obj = Event.New("CheckRealityDistortionAccessibility");
				obj.SetParameter("Object", Object);
				obj.SetParameter("Cell", Cell);
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Mutation", Mutation);
				obj.SetParameter("Device", Device);
				obj.SetParameter("Penetration", Penetration);
				obj.SetFlag("Allow", flag2);
				flag = (!flag3 || Object.FireEvent(obj)) && (!flag4 || Cell.FireEvent(obj));
				flag2 = obj.HasFlag("Allow");
			}
		}
		if (flag)
		{
			bool flag5 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckRealityDistortionAccessibilityEvent>.ID, MinEvent.CascadeLevel);
			bool flag6 = Cell?.WantEvent(PooledEvent<CheckRealityDistortionAccessibilityEvent>.ID, MinEvent.CascadeLevel) ?? false;
			if (flag5 | flag6)
			{
				CheckRealityDistortionAccessibilityEvent checkRealityDistortionAccessibilityEvent = PooledEvent<CheckRealityDistortionAccessibilityEvent>.FromPool();
				checkRealityDistortionAccessibilityEvent.Object = Object;
				checkRealityDistortionAccessibilityEvent.Cell = Cell;
				checkRealityDistortionAccessibilityEvent.Actor = Actor;
				checkRealityDistortionAccessibilityEvent.Mutation = Mutation;
				checkRealityDistortionAccessibilityEvent.Device = Device;
				checkRealityDistortionAccessibilityEvent.Penetration = Penetration;
				checkRealityDistortionAccessibilityEvent.Allow = flag2;
				flag = (!flag5 || Object.HandleEvent(checkRealityDistortionAccessibilityEvent)) && (!flag6 || Cell.HandleEvent(checkRealityDistortionAccessibilityEvent));
				flag2 = checkRealityDistortionAccessibilityEvent.Allow;
			}
		}
		return flag2;
	}
}
