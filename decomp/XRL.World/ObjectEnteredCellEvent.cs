using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ObjectEnteredCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ObjectEnteredCellEvent), null, CountPool, ResetPool);

	private static List<ObjectEnteredCellEvent> Pool;

	private static int PoolCounter;

	public ObjectEnteredCellEvent()
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

	public static void ResetTo(ref ObjectEnteredCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ObjectEnteredCellEvent FromPool()
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

	public static ObjectEnteredCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		ObjectEnteredCellEvent objectEnteredCellEvent = FromPool();
		objectEnteredCellEvent.Object = Object;
		objectEnteredCellEvent.Cell = Cell;
		objectEnteredCellEvent.Forced = Forced;
		objectEnteredCellEvent.System = System;
		objectEnteredCellEvent.IgnoreGravity = IgnoreGravity;
		objectEnteredCellEvent.NoStack = NoStack;
		objectEnteredCellEvent.Direction = Direction;
		objectEnteredCellEvent.Type = Type;
		objectEnteredCellEvent.Dragging = Dragging;
		objectEnteredCellEvent.Actor = Actor;
		objectEnteredCellEvent.ForceSwap = ForceSwap;
		objectEnteredCellEvent.Ignore = Ignore;
		return objectEnteredCellEvent;
	}
}
