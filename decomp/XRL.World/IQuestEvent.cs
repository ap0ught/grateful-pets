namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IQuestEvent : MinEvent
{
	public Quest Quest;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Quest = null;
	}
}
