namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IObjectCreationEvent : MinEvent
{
	public GameObject ReplacementObject;

	public GameObject Object;

	public string Context;

	public GameObject ActiveObject => ReplacementObject ?? Object;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		ReplacementObject = null;
		Object = null;
		Context = null;
	}
}
