namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class RespiresEvent : PooledEvent<RespiresEvent>
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

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("Respires"))
		{
			Event obj = Event.New("Respires");
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<RespiresEvent>.ID, CascadeLevel))
		{
			RespiresEvent respiresEvent = PooledEvent<RespiresEvent>.FromPool();
			respiresEvent.Object = Object;
			flag = Object.HandleEvent(respiresEvent);
		}
		return flag;
	}
}
