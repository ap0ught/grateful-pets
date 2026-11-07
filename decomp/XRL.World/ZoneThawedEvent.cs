namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Singleton)]
public class ZoneThawedEvent : IZoneEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ZoneThawedEvent));

	public new static readonly int CascadeLevel = 15;

	public static readonly ZoneThawedEvent Instance = new ZoneThawedEvent();

	public long TicksFrozen;

	public ZoneThawedEvent()
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

	public override void Reset()
	{
		base.Reset();
		TicksFrozen = 0L;
	}

	public static void Send(Zone Zone, long TicksFrozen = 0L)
	{
		Instance.Zone = Zone;
		Instance.TicksFrozen = TicksFrozen;
		The.Game.HandleEvent(Instance);
		Zone.HandleEvent(Instance);
		Instance.Reset();
	}
}
