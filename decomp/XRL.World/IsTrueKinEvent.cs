namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class IsTrueKinEvent : PooledEvent<IsTrueKinEvent>
{
	public GameObject Object;

	public bool IsTrueKin;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		IsTrueKin = false;
	}

	public static IsTrueKinEvent FromPool(GameObject Object, bool IsTrueKin)
	{
		IsTrueKinEvent isTrueKinEvent = PooledEvent<IsTrueKinEvent>.FromPool();
		isTrueKinEvent.Object = Object;
		isTrueKinEvent.IsTrueKin = IsTrueKin;
		return isTrueKinEvent;
	}

	public static bool Check(GameObject Object)
	{
		bool flag = Object?.genotypeEntry?.IsTrueKin == true;
		bool flag2 = true;
		if (flag2 && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("IsTrueKin"))
		{
			Event obj = Event.New("IsTrueKin");
			obj.SetParameter("Object", Object);
			obj.SetFlag("IsTrueKin", flag);
			flag2 = Object.FireEvent(obj);
			flag = obj.HasFlag("IsTrueKin");
		}
		if (flag2 && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<IsTrueKinEvent>.ID, MinEvent.CascadeLevel))
		{
			IsTrueKinEvent isTrueKinEvent = FromPool(Object, flag);
			flag2 = Object.HandleEvent(isTrueKinEvent);
			flag = isTrueKinEvent.IsTrueKin;
		}
		return flag;
	}
}
