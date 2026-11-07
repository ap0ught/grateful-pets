namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IAIMoveCommandListEvent : IAICommandListEvent
{
	public Cell TargetCell;

	public int StandoffDistance;

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		TargetCell = null;
		StandoffDistance = 0;
	}
}
