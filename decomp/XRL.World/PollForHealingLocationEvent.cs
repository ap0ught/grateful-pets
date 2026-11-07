namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class PollForHealingLocationEvent : PooledEvent<PollForHealingLocationEvent>
{
	public GameObject Actor;

	public Cell Cell;

	public int Value;

	public bool First;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Cell = null;
		Value = 0;
		First = false;
	}

	public static PollForHealingLocationEvent FromPool(GameObject Actor, Cell Cell, bool First = false)
	{
		PollForHealingLocationEvent pollForHealingLocationEvent = PooledEvent<PollForHealingLocationEvent>.FromPool();
		pollForHealingLocationEvent.Actor = Actor;
		pollForHealingLocationEvent.Cell = Cell;
		pollForHealingLocationEvent.Value = 0;
		pollForHealingLocationEvent.First = First;
		return pollForHealingLocationEvent;
	}

	public static int GetFor(GameObject Actor, Cell Cell, bool First = false)
	{
		if (Cell.WantEvent(PooledEvent<PollForHealingLocationEvent>.ID, MinEvent.CascadeLevel))
		{
			PollForHealingLocationEvent pollForHealingLocationEvent = FromPool(Actor, Cell, First);
			Cell.HandleEvent(pollForHealingLocationEvent);
			return pollForHealingLocationEvent.Value;
		}
		return 0;
	}
}
