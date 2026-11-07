namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class ExtremitiesMovedEvent : PooledEvent<ExtremitiesMovedEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Object;

	public string To;

	public bool Involuntary;

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
		To = null;
		Involuntary = false;
	}

	public static ExtremitiesMovedEvent FromPool(GameObject Object, string To = null, bool Involuntary = false)
	{
		ExtremitiesMovedEvent extremitiesMovedEvent = PooledEvent<ExtremitiesMovedEvent>.FromPool();
		extremitiesMovedEvent.Object = Object;
		extremitiesMovedEvent.To = To;
		extremitiesMovedEvent.Involuntary = Involuntary;
		return extremitiesMovedEvent;
	}

	public static void Send(GameObject Object, string To = null, bool Involuntary = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("ExtremitiesMoved"))
		{
			Event obj = Event.New("ExtremitiesMoved");
			obj.SetParameter("Object", Object);
			obj.SetParameter("To", To);
			obj.SetFlag("Involuntary", Involuntary);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<ExtremitiesMovedEvent>.ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, To, Involuntary));
		}
	}
}
