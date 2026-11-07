namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IValueEvent : MinEvent
{
	public GameObject Object;

	public double Value = 0.01;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Value = 0.01;
	}
}
