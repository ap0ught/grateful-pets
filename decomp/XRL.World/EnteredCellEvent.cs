using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EnteredCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EnteredCellEvent), null, CountPool, ResetPool);

	private static List<EnteredCellEvent> Pool;

	private static int PoolCounter;

	public EnteredCellEvent()
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

	public static void ResetTo(ref EnteredCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EnteredCellEvent FromPool()
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

	public static EnteredCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		EnteredCellEvent enteredCellEvent = FromPool();
		enteredCellEvent.Object = Object;
		enteredCellEvent.Cell = Cell;
		enteredCellEvent.Forced = Forced;
		enteredCellEvent.System = System;
		enteredCellEvent.IgnoreGravity = IgnoreGravity;
		enteredCellEvent.NoStack = NoStack;
		enteredCellEvent.Direction = Direction;
		enteredCellEvent.Type = Type;
		enteredCellEvent.Dragging = Dragging;
		enteredCellEvent.Actor = Actor;
		enteredCellEvent.ForceSwap = ForceSwap;
		enteredCellEvent.Ignore = Ignore;
		return enteredCellEvent;
	}
}
