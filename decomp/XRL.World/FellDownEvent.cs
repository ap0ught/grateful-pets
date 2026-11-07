using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class FellDownEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(FellDownEvent), null, CountPool, ResetPool);

	private static List<FellDownEvent> Pool;

	private static int PoolCounter;

	public Cell FromCell;

	public int Distance;

	public FellDownEvent()
	{
		base.ID = ID;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref FellDownEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static FellDownEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

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
		FromCell = null;
		Distance = 0;
	}

	public static FellDownEvent FromPool(GameObject Object, Cell Cell, Cell FromCell, int Distance)
	{
		FellDownEvent fellDownEvent = FromPool();
		fellDownEvent.Object = Object;
		fellDownEvent.Cell = Cell;
		fellDownEvent.Forced = false;
		fellDownEvent.System = false;
		fellDownEvent.IgnoreGravity = false;
		fellDownEvent.NoStack = false;
		fellDownEvent.Direction = "D";
		fellDownEvent.Type = null;
		fellDownEvent.Dragging = null;
		fellDownEvent.Actor = null;
		fellDownEvent.ForceSwap = null;
		fellDownEvent.Ignore = null;
		fellDownEvent.Distance = Distance;
		return fellDownEvent;
	}

	public static void Send(GameObject Object, Cell Cell, Cell FromCell, int Distance)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("FellDown"))
		{
			Event obj = Event.New("FellDown");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Cell", Cell);
			obj.SetParameter("FromCell", FromCell);
			obj.SetParameter("Distance", Distance);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, Cell, FromCell, Distance));
		}
	}
}
