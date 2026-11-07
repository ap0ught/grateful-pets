namespace XRL.World;

[GameEvent(Cascade = 271, Cache = Cache.Pool)]
public class MakeTemporaryEvent : PooledEvent<MakeTemporaryEvent>
{
	public new static readonly int CascadeLevel = 271;

	public GameObject RootObject;

	public int Duration;

	public string TurnInto;

	public GameObject DependsOn;

	public bool RootObjectValidateEveryTurn;

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
		RootObject = null;
		Duration = 0;
		TurnInto = null;
		DependsOn = null;
		RootObjectValidateEveryTurn = false;
	}

	public static bool Send(GameObject RootObject, int Duration = -1, string TurnInto = null, GameObject DependsOn = null, bool RootObjectValidateEveryTurn = false)
	{
		MakeTemporaryEvent makeTemporaryEvent = PooledEvent<MakeTemporaryEvent>.FromPool();
		makeTemporaryEvent.RootObject = RootObject;
		makeTemporaryEvent.Duration = Duration;
		makeTemporaryEvent.TurnInto = TurnInto;
		makeTemporaryEvent.DependsOn = DependsOn;
		makeTemporaryEvent.RootObjectValidateEveryTurn = RootObjectValidateEveryTurn;
		if (!RootObject.HandleEvent(makeTemporaryEvent))
		{
			return false;
		}
		return true;
	}
}
