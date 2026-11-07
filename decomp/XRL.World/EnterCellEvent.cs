using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EnterCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EnterCellEvent), null, CountPool, ResetPool);

	private static List<EnterCellEvent> Pool;

	private static int PoolCounter;

	public EnterCellEvent()
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

	public static void ResetTo(ref EnterCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EnterCellEvent FromPool()
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

	public static EnterCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		EnterCellEvent enterCellEvent = FromPool();
		enterCellEvent.Object = Object;
		enterCellEvent.Cell = Cell;
		enterCellEvent.Forced = Forced;
		enterCellEvent.System = System;
		enterCellEvent.IgnoreGravity = IgnoreGravity;
		enterCellEvent.NoStack = NoStack;
		enterCellEvent.Direction = Direction;
		enterCellEvent.Type = Type;
		enterCellEvent.Dragging = Dragging;
		enterCellEvent.Actor = Actor;
		enterCellEvent.ForceSwap = ForceSwap;
		enterCellEvent.Ignore = Ignore;
		return enterCellEvent;
	}
}
