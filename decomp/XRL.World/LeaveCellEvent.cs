using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class LeaveCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(LeaveCellEvent), null, CountPool, ResetPool);

	private static List<LeaveCellEvent> Pool;

	private static int PoolCounter;

	public LeaveCellEvent()
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

	public static void ResetTo(ref LeaveCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static LeaveCellEvent FromPool()
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

	public static LeaveCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		LeaveCellEvent leaveCellEvent = FromPool();
		leaveCellEvent.Object = Object;
		leaveCellEvent.Cell = Cell;
		leaveCellEvent.Forced = Forced;
		leaveCellEvent.System = System;
		leaveCellEvent.IgnoreGravity = IgnoreGravity;
		leaveCellEvent.NoStack = NoStack;
		leaveCellEvent.Direction = Direction;
		leaveCellEvent.Type = Type;
		leaveCellEvent.Dragging = Dragging;
		leaveCellEvent.Actor = Actor;
		leaveCellEvent.ForceSwap = ForceSwap;
		leaveCellEvent.Ignore = Ignore;
		return leaveCellEvent;
	}
}
