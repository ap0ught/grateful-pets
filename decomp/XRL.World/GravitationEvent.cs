using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GravitationEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GravitationEvent), null, CountPool, ResetPool);

	private static List<GravitationEvent> Pool;

	private static int PoolCounter;

	public GravitationEvent()
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

	public static void ResetTo(ref GravitationEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GravitationEvent FromPool()
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

	public static GravitationEvent FromPool(GameObject Object, Cell Cell)
	{
		GravitationEvent gravitationEvent = FromPool();
		gravitationEvent.Object = Object;
		gravitationEvent.Cell = Cell;
		gravitationEvent.Forced = true;
		gravitationEvent.System = false;
		gravitationEvent.IgnoreGravity = false;
		gravitationEvent.NoStack = false;
		gravitationEvent.Direction = "D";
		gravitationEvent.Type = "Gravitation";
		gravitationEvent.Dragging = null;
		gravitationEvent.Actor = null;
		gravitationEvent.ForceSwap = null;
		gravitationEvent.Ignore = null;
		return gravitationEvent;
	}

	public static void Check(GameObject Object, Cell Cell)
	{
		if (Cell != null && GameObject.Validate(ref Object) && Cell.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Cell.HandleEvent(FromPool(Object, Cell));
		}
	}
}
