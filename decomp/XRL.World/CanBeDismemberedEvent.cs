namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class CanBeDismemberedEvent : PooledEvent<CanBeDismemberedEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public string Attributes;

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
		Attributes = null;
	}

	public static bool Check(GameObject Object, string Attributes = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanBeDismembered"))
		{
			Event obj = Event.New("CanBeDismembered");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Attributes", Attributes);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanBeDismemberedEvent>.ID, CascadeLevel))
		{
			CanBeDismemberedEvent canBeDismemberedEvent = PooledEvent<CanBeDismemberedEvent>.FromPool();
			canBeDismemberedEvent.Object = Object;
			canBeDismemberedEvent.Attributes = Attributes;
			flag = Object.HandleEvent(canBeDismemberedEvent);
		}
		return flag;
	}
}
