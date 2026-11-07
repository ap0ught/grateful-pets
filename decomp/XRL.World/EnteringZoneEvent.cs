using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 32)]
public class EnteringZoneEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EnteringZoneEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 32;

	private static List<EnteringZoneEvent> Pool;

	private static int PoolCounter;

	public Cell Origin;

	public EnteringZoneEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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

	public static void ResetTo(ref EnteringZoneEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EnteringZoneEvent FromPool()
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

	public static EnteringZoneEvent FromPool(GameObject Object, Cell Origin, Cell Destination, bool Forced = false, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = null, string Type = null, GameObject Dragging = null, GameObject Actor = null, GameObject ForceSwap = null, GameObject Ignore = null)
	{
		EnteringZoneEvent enteringZoneEvent = FromPool();
		enteringZoneEvent.Object = Object;
		enteringZoneEvent.Origin = Origin;
		enteringZoneEvent.Cell = Destination;
		enteringZoneEvent.Forced = Forced;
		enteringZoneEvent.System = System;
		enteringZoneEvent.IgnoreGravity = IgnoreGravity;
		enteringZoneEvent.NoStack = NoStack;
		enteringZoneEvent.Direction = Direction;
		enteringZoneEvent.Type = Type;
		enteringZoneEvent.Dragging = Dragging;
		enteringZoneEvent.Actor = Actor;
		enteringZoneEvent.ForceSwap = ForceSwap;
		enteringZoneEvent.Ignore = Ignore;
		return enteringZoneEvent;
	}
}
