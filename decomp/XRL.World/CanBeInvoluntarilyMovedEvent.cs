namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class CanBeInvoluntarilyMovedEvent : PooledEvent<CanBeInvoluntarilyMovedEvent>
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

	public static CanBeInvoluntarilyMovedEvent FromPool(GameObject Object)
	{
		CanBeInvoluntarilyMovedEvent canBeInvoluntarilyMovedEvent = PooledEvent<CanBeInvoluntarilyMovedEvent>.FromPool();
		canBeInvoluntarilyMovedEvent.Object = Object;
		return canBeInvoluntarilyMovedEvent;
	}

	public static bool Check(GameObject Object)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanBeInvoluntarilyMoved"))
		{
			Event obj = Event.New("CanBeInvoluntarilyMoved");
			obj.SetParameter("Object", Object);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanBeInvoluntarilyMovedEvent>.ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object));
		}
		return flag;
	}
}
