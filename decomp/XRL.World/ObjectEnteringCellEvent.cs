using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ObjectEnteringCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ObjectEnteringCellEvent), null, CountPool, ResetPool);

	private static List<ObjectEnteringCellEvent> Pool;

	private static int PoolCounter;

	public ObjectEnteringCellEvent()
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

	public static void ResetTo(ref ObjectEnteringCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ObjectEnteringCellEvent FromPool()
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

	public static ObjectEnteringCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		ObjectEnteringCellEvent objectEnteringCellEvent = FromPool();
		objectEnteringCellEvent.Object = Object;
		objectEnteringCellEvent.Cell = Cell;
		objectEnteringCellEvent.Forced = Forced;
		objectEnteringCellEvent.System = System;
		objectEnteringCellEvent.IgnoreGravity = IgnoreGravity;
		objectEnteringCellEvent.NoStack = NoStack;
		objectEnteringCellEvent.Direction = Direction;
		objectEnteringCellEvent.Type = Type;
		objectEnteringCellEvent.Dragging = Dragging;
		objectEnteringCellEvent.Actor = Actor;
		objectEnteringCellEvent.ForceSwap = ForceSwap;
		objectEnteringCellEvent.Ignore = Ignore;
		return objectEnteringCellEvent;
	}
}
