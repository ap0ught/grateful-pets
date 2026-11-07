namespace XRL.World;

[GameEvent(Base = true, Cascade = 0)]
public abstract class IShallowZoneEvent : IZoneEvent
{
	public new static readonly int CascadeLevel;

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
