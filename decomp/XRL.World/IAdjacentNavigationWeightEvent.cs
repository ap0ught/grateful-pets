namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IAdjacentNavigationWeightEvent : INavigationWeightEvent
{
	public Cell AdjacentCell;

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
		AdjacentCell = null;
	}

	public override void ApplyTo(INavigationWeightEvent E)
	{
		base.ApplyTo(E);
		if (E is IAdjacentNavigationWeightEvent adjacentNavigationWeightEvent)
		{
			adjacentNavigationWeightEvent.AdjacentCell = AdjacentCell;
		}
	}
}
