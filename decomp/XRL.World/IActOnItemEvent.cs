namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IActOnItemEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Item;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Item = null;
	}
}
