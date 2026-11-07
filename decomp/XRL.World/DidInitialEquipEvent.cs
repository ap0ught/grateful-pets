namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class DidInitialEquipEvent : PooledEvent<DidInitialEquipEvent>
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
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("DidInitialEquip"))
		{
			Event obj = Event.New("DidInitialEquip");
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<DidInitialEquipEvent>.ID, CascadeLevel))
		{
			DidInitialEquipEvent didInitialEquipEvent = PooledEvent<DidInitialEquipEvent>.FromPool();
			didInitialEquipEvent.Object = Object;
			flag = Object.HandleEvent(didInitialEquipEvent);
		}
	}
}
