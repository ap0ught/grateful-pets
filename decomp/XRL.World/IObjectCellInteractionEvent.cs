namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IObjectCellInteractionEvent : MinEvent
{
	public GameObject Object;

	public Cell Cell;

	public bool Forced;

	public bool System;

	public bool IgnoreGravity;

	public bool NoStack;

	public string Direction;

	public string Type;

	public GameObject Dragging;

	public GameObject Actor;

	public GameObject ForceSwap;

	public GameObject Ignore;

	public GameObject Blocking;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Cell = null;
		Forced = false;
		System = false;
		IgnoreGravity = false;
		NoStack = false;
		Direction = null;
		Type = null;
		Dragging = null;
		Actor = null;
		ForceSwap = null;
		Ignore = null;
		Blocking = null;
	}
}
