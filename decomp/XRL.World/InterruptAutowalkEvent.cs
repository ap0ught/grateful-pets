namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class InterruptAutowalkEvent : PooledEvent<InterruptAutowalkEvent>
{
	public GameObject Actor;

	public Cell Cell;

	public string Because;

	public GameObject IndicateObject;

	public Cell IndicateCell;

	public bool AsThreat;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Cell = null;
		Because = null;
		IndicateObject = null;
		IndicateCell = null;
		AsThreat = false;
	}

	public static bool Check(GameObject Actor, Cell Cell, out string Because, out GameObject IndicateObject, out Cell IndicateCell, out bool AsThreat)
	{
		Because = null;
		IndicateObject = null;
		IndicateCell = null;
		AsThreat = false;
		if (Cell == null)
		{
			return true;
		}
		bool flag = true;
		if (flag && Cell.WantEvent(PooledEvent<InterruptAutowalkEvent>.ID, MinEvent.CascadeLevel))
		{
			InterruptAutowalkEvent interruptAutowalkEvent = PooledEvent<InterruptAutowalkEvent>.FromPool();
			interruptAutowalkEvent.Actor = Actor;
			interruptAutowalkEvent.Cell = Cell;
			flag = Cell.HandleEvent(interruptAutowalkEvent);
			Because = interruptAutowalkEvent.Because;
			IndicateObject = interruptAutowalkEvent.IndicateObject;
			IndicateCell = interruptAutowalkEvent.IndicateCell;
			AsThreat = interruptAutowalkEvent.AsThreat;
		}
		return !flag;
	}

	public static bool Check(GameObject Actor, Cell Cell)
	{
		string Because;
		GameObject IndicateObject;
		Cell IndicateCell;
		bool AsThreat;
		return Check(Actor, Cell, out Because, out IndicateObject, out IndicateCell, out AsThreat);
	}
}
