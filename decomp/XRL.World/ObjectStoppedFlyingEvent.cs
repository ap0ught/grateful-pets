using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ObjectStoppedFlyingEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ObjectStoppedFlyingEvent), null, CountPool, ResetPool);

	private static List<ObjectStoppedFlyingEvent> Pool;

	private static int PoolCounter;

	public ObjectStoppedFlyingEvent()
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

	public static void ResetTo(ref ObjectStoppedFlyingEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ObjectStoppedFlyingEvent FromPool()
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
		ObjectStoppedFlyingEvent objectStoppedFlyingEvent = FromPool();
		objectStoppedFlyingEvent.Object = Object;
		objectStoppedFlyingEvent.Cell = Object.CurrentCell;
		objectStoppedFlyingEvent.Forced = false;
		objectStoppedFlyingEvent.System = false;
		objectStoppedFlyingEvent.IgnoreGravity = false;
		objectStoppedFlyingEvent.NoStack = false;
		objectStoppedFlyingEvent.Direction = null;
		objectStoppedFlyingEvent.Type = null;
		objectStoppedFlyingEvent.Dragging = null;
		objectStoppedFlyingEvent.Actor = Actor ?? Object;
		objectStoppedFlyingEvent.ForceSwap = null;
		objectStoppedFlyingEvent.Ignore = null;
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(objectStoppedFlyingEvent))
		{
			return false;
		}
		if (objectStoppedFlyingEvent.Cell != null && objectStoppedFlyingEvent.Cell.WantEvent(ID, MinEvent.CascadeLevel) && !objectStoppedFlyingEvent.Cell.HandleEvent(objectStoppedFlyingEvent))
		{
			return false;
		}
		return true;
	}
}
