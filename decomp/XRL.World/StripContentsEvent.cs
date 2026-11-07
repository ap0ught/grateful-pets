namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class StripContentsEvent : PooledEvent<StripContentsEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Object;

	public bool KeepNatural;

	public bool Silent;

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
		Object = null;
		KeepNatural = false;
		Silent = false;
	}

	public static StripContentsEvent FromPool(GameObject Object, bool KeepNatural = false, bool Silent = false)
	{
		StripContentsEvent stripContentsEvent = PooledEvent<StripContentsEvent>.FromPool();
		stripContentsEvent.Object = Object;
		stripContentsEvent.KeepNatural = KeepNatural;
		stripContentsEvent.Silent = Silent;
		return stripContentsEvent;
	}
}
