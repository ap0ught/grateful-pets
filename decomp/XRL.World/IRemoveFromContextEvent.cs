namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IRemoveFromContextEvent : MinEvent
{
	public GameObject Object;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
	}
}
