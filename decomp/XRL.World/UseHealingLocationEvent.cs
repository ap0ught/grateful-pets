namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class UseHealingLocationEvent : PooledEvent<UseHealingLocationEvent>
{
	public GameObject Actor;

	public Cell Cell;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Cell = null;
	}

	public static UseHealingLocationEvent FromPool(GameObject Actor, Cell Cell)
	{
		UseHealingLocationEvent useHealingLocationEvent = PooledEvent<UseHealingLocationEvent>.FromPool();
		useHealingLocationEvent.Actor = Actor;
		useHealingLocationEvent.Cell = Cell;
		return useHealingLocationEvent;
	}

	public static void Send(GameObject Actor, Cell Cell)
	{
		Cell.HandleEvent(FromPool(Actor, Cell));
	}
}
