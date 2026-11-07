namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class DidReequipEvent : PooledEvent<DidReequipEvent>
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

	public static void Send(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("DidReequip"))
		{
			Event obj = Event.New("DidReequip");
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<DidReequipEvent>.ID, CascadeLevel))
		{
			DidReequipEvent didReequipEvent = PooledEvent<DidReequipEvent>.FromPool();
			didReequipEvent.Object = Object;
			flag = Object.HandleEvent(didReequipEvent);
		}
	}
}
