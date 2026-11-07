namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class InteriorZoneBuiltEvent : PooledEvent<InteriorZoneBuiltEvent>
{
	public new static readonly int CascadeLevel = 15;

	public InteriorZone Zone;

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
		Zone = null;
	}

	public static void Send(GameObject Object, InteriorZone Zone)
	{
		if (!GameObject.Validate(Object))
		{
			return;
		}
		if (Object.HasRegisteredEvent("InteriorZoneBuilt"))
		{
			Event e = Event.New("InteriorZoneBuilt", "Zone", Zone);
			if (!Object.FireEvent(e))
			{
				return;
			}
		}
		if (Object.WantEvent(PooledEvent<InteriorZoneBuiltEvent>.ID, CascadeLevel))
		{
			InteriorZoneBuiltEvent interiorZoneBuiltEvent = PooledEvent<InteriorZoneBuiltEvent>.FromPool();
			interiorZoneBuiltEvent.Zone = Zone;
			Object.HandleEvent(interiorZoneBuiltEvent);
		}
	}
}
