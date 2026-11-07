using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EnteringCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EnteringCellEvent), null, CountPool, ResetPool);

	private static List<EnteringCellEvent> Pool;

	private static int PoolCounter;

	public EnteringCellEvent()
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

	public static void ResetTo(ref EnteringCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EnteringCellEvent FromPool()
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

	public static EnteringCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		EnteringCellEvent enteringCellEvent = FromPool();
		enteringCellEvent.Object = Object;
		enteringCellEvent.Cell = Cell;
		enteringCellEvent.Forced = Forced;
		enteringCellEvent.System = System;
		enteringCellEvent.IgnoreGravity = IgnoreGravity;
		enteringCellEvent.NoStack = NoStack;
		enteringCellEvent.Direction = Direction;
		enteringCellEvent.Type = Type;
		enteringCellEvent.Dragging = Dragging;
		enteringCellEvent.Actor = Actor;
		enteringCellEvent.ForceSwap = ForceSwap;
		enteringCellEvent.Ignore = Ignore;
		return enteringCellEvent;
	}
}
