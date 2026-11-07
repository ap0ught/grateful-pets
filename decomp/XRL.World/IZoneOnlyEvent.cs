namespace XRL.World;

[GameEvent(Base = true, Cascade = 32)]
public abstract class IZoneOnlyEvent : IZoneEvent
{
	public new static readonly int CascadeLevel = 32;

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
}
