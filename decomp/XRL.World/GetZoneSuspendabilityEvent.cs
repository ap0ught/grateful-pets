namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Singleton)]
public class GetZoneSuspendabilityEvent : IZoneEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetZoneSuspendabilityEvent));

	public new static readonly int CascadeLevel = 0;

	public static readonly GetZoneSuspendabilityEvent Instance = new GetZoneSuspendabilityEvent();

	public Suspendability Suspendability = Suspendability.Suspendable;

	public GetZoneSuspendabilityEvent()
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
		Suspendability = Suspendability.Active;
	}

	public static Suspendability GetFor(Zone Zone, Suspendability Suspendability = Suspendability.Suspendable)
	{
		Instance.Zone = Zone;
		Instance.Suspendability = Suspendability;
		The.Game.HandleEvent(Instance);
		Zone.HandleEvent(Instance);
		Suspendability suspendability = Instance.Suspendability;
		Instance.Reset();
		return suspendability;
	}
}
