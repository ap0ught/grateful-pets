namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ReplaceInContextEvent : PooledEvent<ReplaceInContextEvent>
{
	public GameObject Object;

	public GameObject Replacement;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Replacement = null;
	}

	public static ReplaceInContextEvent FromPool(GameObject Object, GameObject Replacement)
	{
		ReplaceInContextEvent replaceInContextEvent = PooledEvent<ReplaceInContextEvent>.FromPool();
		replaceInContextEvent.Object = Object;
		replaceInContextEvent.Replacement = Replacement;
		return replaceInContextEvent;
	}

	public static void Send(GameObject Object, GameObject Replacement)
	{
		if (GameObject.Validate(ref Object))
		{
			Object.HandleEvent(FromPool(Object, Replacement));
		}
	}
}
