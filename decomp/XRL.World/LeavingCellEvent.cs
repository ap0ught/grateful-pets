using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class LeavingCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(LeavingCellEvent), null, CountPool, ResetPool);

	private static List<LeavingCellEvent> Pool;

	private static int PoolCounter;

	public LeavingCellEvent()
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

	public static void ResetTo(ref LeavingCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static LeavingCellEvent FromPool()
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

	public static LeavingCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		LeavingCellEvent leavingCellEvent = FromPool();
		leavingCellEvent.Object = Object;
		leavingCellEvent.Cell = Cell;
		leavingCellEvent.Forced = Forced;
		leavingCellEvent.System = System;
		leavingCellEvent.IgnoreGravity = IgnoreGravity;
		leavingCellEvent.NoStack = NoStack;
		leavingCellEvent.Direction = Direction;
		leavingCellEvent.Type = Type;
		leavingCellEvent.Dragging = Dragging;
		leavingCellEvent.Actor = Actor;
		leavingCellEvent.ForceSwap = ForceSwap;
		leavingCellEvent.Ignore = Ignore;
		return leavingCellEvent;
	}
}
