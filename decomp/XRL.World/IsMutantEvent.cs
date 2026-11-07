namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class IsMutantEvent : PooledEvent<IsMutantEvent>
{
	public GameObject Object;

	public bool IsMutant;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		IsMutant = false;
	}

	public static bool Check(GameObject Object)
	{
		bool flag = Object?.genotypeEntry?.IsMutant ?? (Object.IsCreature && !Object.HasTagOrProperty("NonMutant"));
		bool flag2 = true;
		if (flag2 && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("IsMutant"))
		{
			Event obj = Event.New("IsMutant");
			obj.SetParameter("Object", Object);
			obj.SetFlag("IsMutant", flag);
			flag2 = Object.FireEvent(obj);
			flag = obj.HasFlag("IsMutant");
		}
		if (flag2 && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<IsMutantEvent>.ID, MinEvent.CascadeLevel))
		{
			IsMutantEvent isMutantEvent = PooledEvent<IsMutantEvent>.FromPool();
			isMutantEvent.Object = Object;
			isMutantEvent.IsMutant = flag;
			flag2 = Object.HandleEvent(isMutantEvent);
			flag = isMutantEvent.IsMutant;
		}
		return flag;
	}
}
