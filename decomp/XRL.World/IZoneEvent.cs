namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IZoneEvent : MinEvent
{
	public Zone Zone;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Zone = null;
	}
}
