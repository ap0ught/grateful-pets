using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class LeftCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(LeftCellEvent), null, CountPool, ResetPool);

	private static List<LeftCellEvent> Pool;

	private static int PoolCounter;

	public LeftCellEvent()
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

	public static void ResetTo(ref LeftCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static LeftCellEvent FromPool()
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

	public static LeftCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		LeftCellEvent leftCellEvent = FromPool();
		leftCellEvent.Object = Object;
		leftCellEvent.Cell = Cell;
		leftCellEvent.Forced = Forced;
		leftCellEvent.System = System;
		leftCellEvent.IgnoreGravity = IgnoreGravity;
		leftCellEvent.NoStack = NoStack;
		leftCellEvent.Direction = Direction;
		leftCellEvent.Type = Type;
		leftCellEvent.Dragging = Dragging;
		leftCellEvent.Actor = Actor;
		leftCellEvent.ForceSwap = ForceSwap;
		leftCellEvent.Ignore = Ignore;
		return leftCellEvent;
	}
}
