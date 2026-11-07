using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ObjectLeavingCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ObjectLeavingCellEvent), null, CountPool, ResetPool);

	private static List<ObjectLeavingCellEvent> Pool;

	private static int PoolCounter;

	public ObjectLeavingCellEvent()
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

	public static void ResetTo(ref ObjectLeavingCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ObjectLeavingCellEvent FromPool()
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

	public static ObjectLeavingCellEvent FromPool(GameObject Object, Cell Cell, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		ObjectLeavingCellEvent objectLeavingCellEvent = FromPool();
		objectLeavingCellEvent.Object = Object;
		objectLeavingCellEvent.Cell = Cell;
		objectLeavingCellEvent.Forced = Forced;
		objectLeavingCellEvent.System = System;
		objectLeavingCellEvent.IgnoreGravity = IgnoreGravity;
		objectLeavingCellEvent.NoStack = NoStack;
		objectLeavingCellEvent.Direction = Direction;
		objectLeavingCellEvent.Type = Type;
		objectLeavingCellEvent.Dragging = Dragging;
		objectLeavingCellEvent.Actor = Actor;
		objectLeavingCellEvent.ForceSwap = ForceSwap;
		objectLeavingCellEvent.Ignore = Ignore;
		return objectLeavingCellEvent;
	}
}
