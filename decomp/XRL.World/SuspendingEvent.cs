namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class SuspendingEvent : IZoneEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(SuspendingEvent));

	public new static readonly int CascadeLevel = 15;

	public static readonly SuspendingEvent Instance = new SuspendingEvent();

	public SuspendingEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public static void Send(Zone Zone)
	{
		Instance.Zone = Zone;
		The.Game.HandleEvent(Instance);
		Zone.HandleEvent(Instance);
		Instance.Reset();
	}
}
