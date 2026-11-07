namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.None)]
public class SynchronizeExistenceEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(SynchronizeExistenceEvent));

	public new static readonly int CascadeLevel = 15;

	public SynchronizeExistenceEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}
}
