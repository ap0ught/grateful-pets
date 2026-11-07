namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class AttemptToLandEvent : PooledEvent<AttemptToLandEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Actor;

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
		Actor = null;
	}

	public static AttemptToLandEvent FromPool(GameObject Actor)
	{
		AttemptToLandEvent attemptToLandEvent = PooledEvent<AttemptToLandEvent>.FromPool();
		attemptToLandEvent.Actor = Actor;
		return attemptToLandEvent;
	}

	public static bool Check(GameObject Actor)
	{
		if (GameObject.Validate(Actor) && Actor.HasRegisteredEvent("AttemptToLand") && !Actor.FireEvent(Event.New("AttemptToLand", "Actor", Actor)))
		{
			return true;
		}
		if (GameObject.Validate(Actor) && Actor.WantEvent(PooledEvent<AttemptToLandEvent>.ID, CascadeLevel) && !Actor.HandleEvent(FromPool(Actor)))
		{
			return true;
		}
		return false;
	}
}
