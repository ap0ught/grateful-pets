using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterMoveFailedEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterMoveFailedEvent), null, CountPool, ResetPool);

	private static List<AfterMoveFailedEvent> Pool;

	private static int PoolCounter;

	private static ImmutableEvent RegisteredInstance = new ImmutableEvent("MoveFailed");

	public Cell Origin;

	public AfterMoveFailedEvent()
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

	public static void ResetTo(ref AfterMoveFailedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterMoveFailedEvent FromPool()
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
		Origin = null;
	}

	public static void Send(GameObject Object, Cell Origin, Cell Destination, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		Object.FireRegisteredEvent(RegisteredInstance);
		AfterMoveFailedEvent E = FromPool();
		E.Object = Object;
		E.Origin = Origin;
		E.Cell = Destination;
		E.Forced = Forced;
		E.System = System;
		E.IgnoreGravity = IgnoreGravity;
		E.NoStack = NoStack;
		E.Direction = Direction;
		E.Type = Type;
		E.Dragging = Dragging;
		E.Actor = Actor;
		E.ForceSwap = ForceSwap;
		E.Ignore = Ignore;
		Object.HandleEvent(E);
		ResetTo(ref E);
	}
}
