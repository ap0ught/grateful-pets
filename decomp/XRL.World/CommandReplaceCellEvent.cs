namespace XRL.World;

[GameEvent(Cascade = 256, Cache = Cache.Pool)]
public class CommandReplaceCellEvent : PooledEvent<CommandReplaceCellEvent>
{
	public new static readonly int CascadeLevel = 256;

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

	public static void Execute(GameObject Actor)
	{
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<CommandReplaceCellEvent>.ID, CascadeLevel))
		{
			CommandReplaceCellEvent commandReplaceCellEvent = PooledEvent<CommandReplaceCellEvent>.FromPool();
			commandReplaceCellEvent.Actor = Actor;
			Actor.HandleEvent(commandReplaceCellEvent);
		}
	}
}
