namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IObjectInventoryInteractionEvent : MinEvent
{
	public GameObject Object;

	public IInventory Inventory;

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

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Inventory = null;
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
	}
}
