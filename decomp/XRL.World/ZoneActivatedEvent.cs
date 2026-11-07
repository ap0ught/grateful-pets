namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class ZoneActivatedEvent : IZoneEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ZoneActivatedEvent));

	public new static readonly int CascadeLevel = 15;

	public static readonly ZoneActivatedEvent Instance = new ZoneActivatedEvent();

	public ZoneActivatedEvent()
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
