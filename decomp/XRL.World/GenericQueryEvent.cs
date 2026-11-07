namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GenericQueryEvent : PooledEvent<GenericQueryEvent>
{
	public GameObject Object;

	public GameObject Subject;

	public GameObject Source;

	public string Query;

	public int Level;

	public bool Result;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Subject = null;
		Source = null;
		Query = null;
		Level = 0;
		Result = false;
	}

	public static bool Check(GameObject Object, string Query, GameObject Subject = null, GameObject Source = null, int Level = 0, bool BaseResult = false)
	{
		GenericQueryEvent genericQueryEvent = PooledEvent<GenericQueryEvent>.FromPool();
		genericQueryEvent.Object = Object;
		genericQueryEvent.Subject = Subject;
		genericQueryEvent.Source = Source;
		genericQueryEvent.Query = Query;
		genericQueryEvent.Level = Level;
		genericQueryEvent.Result = BaseResult;
		Object.HandleEvent(genericQueryEvent);
		The.Game.HandleEvent(genericQueryEvent);
		return genericQueryEvent.Result;
	}
}
