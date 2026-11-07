namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class GeneralAmnestyEvent : SingletonEvent<GeneralAmnestyEvent>
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
			Z.HandleEvent(SingletonEvent<GeneralAmnestyEvent>.Instance);
			SingletonEvent<GeneralAmnestyEvent>.Instance.Reset();
		}
	}

	public static void Send()
	{
		The.ZoneManager.HandleEvent(SingletonEvent<GeneralAmnestyEvent>.Instance);
		SingletonEvent<GeneralAmnestyEvent>.Instance.Reset();
	}
}
