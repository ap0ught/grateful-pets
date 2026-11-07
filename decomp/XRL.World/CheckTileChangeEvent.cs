namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckTileChangeEvent : PooledEvent<CheckTileChangeEvent>
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

	public static CheckTileChangeEvent FromPool(GameObject Object)
	{
		CheckTileChangeEvent checkTileChangeEvent = PooledEvent<CheckTileChangeEvent>.FromPool();
		checkTileChangeEvent.Object = Object;
		return checkTileChangeEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CheckTileChangeEvent>.ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object)))
		{
			return false;
		}
		return true;
	}
}
