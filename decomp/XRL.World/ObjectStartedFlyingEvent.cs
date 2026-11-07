using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ObjectStartedFlyingEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ObjectStartedFlyingEvent), null, CountPool, ResetPool);

	private static List<ObjectStartedFlyingEvent> Pool;

	private static int PoolCounter;

	public ObjectStartedFlyingEvent()
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

	public static void ResetTo(ref ObjectStartedFlyingEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ObjectStartedFlyingEvent FromPool()
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

	public static bool SendFor(GameObject Object, GameObject Actor = null)
	{
		ObjectStartedFlyingEvent objectStartedFlyingEvent = FromPool();
		objectStartedFlyingEvent.Object = Object;
		objectStartedFlyingEvent.Cell = Object.CurrentCell;
		objectStartedFlyingEvent.Forced = false;
		objectStartedFlyingEvent.System = false;
		objectStartedFlyingEvent.IgnoreGravity = false;
		objectStartedFlyingEvent.NoStack = false;
		objectStartedFlyingEvent.Direction = null;
		objectStartedFlyingEvent.Type = null;
		objectStartedFlyingEvent.Dragging = null;
		objectStartedFlyingEvent.Actor = Actor ?? Object;
		objectStartedFlyingEvent.ForceSwap = null;
		objectStartedFlyingEvent.Ignore = null;
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(objectStartedFlyingEvent))
		{
			return false;
		}
		if (objectStartedFlyingEvent.Cell != null && objectStartedFlyingEvent.Cell.WantEvent(ID, MinEvent.CascadeLevel) && !objectStartedFlyingEvent.Cell.HandleEvent(objectStartedFlyingEvent))
		{
			return false;
		}
		return true;
	}
}
