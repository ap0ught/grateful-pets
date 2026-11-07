namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IReplicationEvent : MinEvent
{
	public GameObject Object;

	public GameObject Actor;

	public string Context;

	public bool Temporary;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Actor = null;
		Context = null;
		Temporary = false;
	}
}
