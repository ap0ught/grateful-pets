namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class TransparentToEMPEvent : PooledEvent<TransparentToEMPEvent>
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
		if (flag && GameObject.Validate(Object) && Object.WantEvent(PooledEvent<TransparentToEMPEvent>.ID, MinEvent.CascadeLevel))
		{
			TransparentToEMPEvent transparentToEMPEvent = PooledEvent<TransparentToEMPEvent>.FromPool();
			transparentToEMPEvent.Object = Object;
			flag = Object.HandleEvent(transparentToEMPEvent);
		}
		return flag;
	}
}
