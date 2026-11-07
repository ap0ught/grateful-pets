namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class HealsNaturallyEvent : PooledEvent<HealsNaturallyEvent>
{
	public GameObject Object;

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
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<HealsNaturallyEvent>.ID, MinEvent.CascadeLevel))
		{
			HealsNaturallyEvent healsNaturallyEvent = PooledEvent<HealsNaturallyEvent>.FromPool();
			healsNaturallyEvent.Object = Object;
			flag = Object.HandleEvent(healsNaturallyEvent);
		}
		return !flag;
	}
}
