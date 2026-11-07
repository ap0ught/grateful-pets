namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class TakeOnRoleEvent : PooledEvent<TakeOnRoleEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public string Role;

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
		Role = null;
	}

	public static void Send(GameObject Object, string Role)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("TakeOnRole"))
		{
			Event obj = Event.New("TakeOnRole");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Role", Role);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<TakeOnRoleEvent>.ID, CascadeLevel))
		{
			TakeOnRoleEvent takeOnRoleEvent = PooledEvent<TakeOnRoleEvent>.FromPool();
			takeOnRoleEvent.Object = Object;
			takeOnRoleEvent.Role = Role;
			flag = Object.HandleEvent(takeOnRoleEvent);
		}
	}
}
