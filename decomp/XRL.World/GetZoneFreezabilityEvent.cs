namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Singleton)]
public class GetZoneFreezabilityEvent : IZoneEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetZoneFreezabilityEvent));

	public new static readonly int CascadeLevel = 0;

	public static readonly GetZoneFreezabilityEvent Instance = new GetZoneFreezabilityEvent();

	public Freezability Freezability = Freezability.Freezable;

	public GetZoneFreezabilityEvent()
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
		Freezability = Freezability.TooRecentlyActive;
	}

	public static Freezability GetFor(Zone Zone, Freezability Freezability = Freezability.Freezable)
	{
		Instance.Zone = Zone;
		Instance.Freezability = Freezability;
		The.Game.HandleEvent(Instance);
		Zone.HandleEvent(Instance);
		Freezability freezability = Instance.Freezability;
		Instance.Reset();
		return freezability;
	}
}
