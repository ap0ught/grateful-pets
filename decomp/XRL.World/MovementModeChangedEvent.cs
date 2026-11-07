namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class MovementModeChangedEvent : PooledEvent<MovementModeChangedEvent>
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

	public static void Send(GameObject Object, string To = null, bool Involuntary = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("MovementModeChanged"))
		{
			Event obj = Event.New("MovementModeChanged");
			obj.SetParameter("Object", Object);
			obj.SetParameter("To", To);
			obj.SetFlag("Involuntary", Involuntary);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<MovementModeChangedEvent>.ID, CascadeLevel))
		{
			MovementModeChangedEvent movementModeChangedEvent = PooledEvent<MovementModeChangedEvent>.FromPool();
			movementModeChangedEvent.Object = Object;
			movementModeChangedEvent.To = To;
			movementModeChangedEvent.Involuntary = Involuntary;
			flag = Object.HandleEvent(movementModeChangedEvent);
		}
	}
}
