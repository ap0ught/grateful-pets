namespace XRL.World;

[GameEvent(Base = true, Cascade = 17)]
public abstract class IAIItemCommandListEvent : IAICommandListEvent
{
	public new static readonly int CascadeLevel = 17;

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
