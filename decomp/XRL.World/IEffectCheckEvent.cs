namespace XRL.World;

[GameEvent(Base = true, Cascade = 0)]
public abstract class IEffectCheckEvent : MinEvent
{
	public new static readonly int CascadeLevel;

	public string Name;

	public int Duration;

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
		Name = null;
		Duration = 0;
	}
}
