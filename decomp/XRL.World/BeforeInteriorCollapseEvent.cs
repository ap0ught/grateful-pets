namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class BeforeInteriorCollapseEvent : PooledEvent<BeforeInteriorCollapseEvent>
{
	public new static readonly int CascadeLevel = 17;

	public InteriorZone Zone;

	public GameObject Object;

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
		Object = null;
	}
}
