namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class AfterGameLoadedEvent : SingletonEvent<AfterGameLoadedEvent>
{
	public new static readonly int CascadeLevel = 15;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public static void Send(Zone Z)
	{
		if (Z != null)
		{
			Z.HandleEvent(SingletonEvent<AfterGameLoadedEvent>.Instance);
			SingletonEvent<AfterGameLoadedEvent>.Instance.Reset();
		}
	}

	public static void Send(XRLGame Game)
	{
		Game.HandleEvent(SingletonEvent<AfterGameLoadedEvent>.Instance);
		Game.ZoneManager.HandleEvent(SingletonEvent<AfterGameLoadedEvent>.Instance);
		SingletonEvent<AfterGameLoadedEvent>.Instance.Reset();
	}
}
