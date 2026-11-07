using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class MissileTraversingCellEvent : IObjectCellInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(MissileTraversingCellEvent), null, CountPool, ResetPool);

	private static List<MissileTraversingCellEvent> Pool;

	private static int PoolCounter;

	public bool Thrown;

	public MissileTraversingCellEvent()
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

	public static void ResetTo(ref MissileTraversingCellEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static MissileTraversingCellEvent FromPool()
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
		Thrown = false;
	}

	public static bool Check(GameObject Object, Cell Cell, string Direction = null, GameObject Actor = null, bool Thrown = false)
	{
		bool flag = true;
		if (flag && Cell != null && Cell.HasObjectWithRegisteredEvent("MissileTraversingCell"))
		{
			Event obj = Event.New("MissileTraversingCell");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Cell", Cell);
			obj.SetParameter("Direction", Direction);
			obj.SetParameter("Actor", Actor);
			obj.SetFlag("Thrown", Thrown);
			flag = Cell.FireEvent(obj);
		}
		if (flag && Cell != null && Cell.WantEvent(ID, MinEvent.CascadeLevel))
		{
			MissileTraversingCellEvent missileTraversingCellEvent = FromPool();
			missileTraversingCellEvent.Object = Object;
			missileTraversingCellEvent.Cell = Cell;
			missileTraversingCellEvent.Forced = false;
			missileTraversingCellEvent.System = false;
			missileTraversingCellEvent.IgnoreGravity = true;
			missileTraversingCellEvent.NoStack = true;
			missileTraversingCellEvent.Direction = Direction;
			missileTraversingCellEvent.Type = (Thrown ? "Throw" : "Missile");
			missileTraversingCellEvent.Dragging = null;
			missileTraversingCellEvent.Actor = Actor;
			missileTraversingCellEvent.ForceSwap = null;
			missileTraversingCellEvent.Ignore = null;
			missileTraversingCellEvent.Thrown = Thrown;
			flag = Cell.HandleEvent(missileTraversingCellEvent);
		}
		return flag;
	}
}
