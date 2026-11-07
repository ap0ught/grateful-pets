namespace XRL.World;

[GameEvent(Cascade = 271, Cache = Cache.Singleton)]
public class FindObjectByIdEvent : SingletonEvent<FindObjectByIdEvent>
{
	public new static readonly int CascadeLevel = 271;

	public string FindID;

	public int FindBaseID;

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
		FindID = null;
		FindBaseID = 0;
		Object = null;
	}

	public static GameObject Find(GameObject From, string ID)
	{
		SingletonEvent<FindObjectByIdEvent>.Instance.FindID = ID;
		SingletonEvent<FindObjectByIdEvent>.Instance.Object = null;
		From.HandleEvent(SingletonEvent<FindObjectByIdEvent>.Instance);
		GameObject result = SingletonEvent<FindObjectByIdEvent>.Instance.Object;
		SingletonEvent<FindObjectByIdEvent>.Instance.Reset();
		return result;
	}

	public static GameObject Find(GameObject From, int ID)
	{
		SingletonEvent<FindObjectByIdEvent>.Instance.FindBaseID = ID;
		SingletonEvent<FindObjectByIdEvent>.Instance.Object = null;
		From.HandleEvent(SingletonEvent<FindObjectByIdEvent>.Instance);
		GameObject result = SingletonEvent<FindObjectByIdEvent>.Instance.Object;
		SingletonEvent<FindObjectByIdEvent>.Instance.Reset();
		return result;
	}
}
