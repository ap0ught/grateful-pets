namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class StackCountChangedEvent : PooledEvent<StackCountChangedEvent>
{
	public GameObject Object;

	public int OldValue;

	public int NewValue;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		OldValue = 0;
		NewValue = 0;
	}

	public static void Send(GameObject Object, int OldValue, int NewValue)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("StackCountChanged"))
		{
			Event obj = Event.New("StackCountChanged");
			obj.SetParameter("Object", Object);
			obj.SetParameter("OldValue", OldValue);
			obj.SetParameter("NewValue", NewValue);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<StackCountChangedEvent>.ID, MinEvent.CascadeLevel))
		{
			StackCountChangedEvent stackCountChangedEvent = PooledEvent<StackCountChangedEvent>.FromPool();
			stackCountChangedEvent.Object = Object;
			stackCountChangedEvent.OldValue = OldValue;
			stackCountChangedEvent.NewValue = NewValue;
			flag = Object.HandleEvent(stackCountChangedEvent);
		}
	}
}
