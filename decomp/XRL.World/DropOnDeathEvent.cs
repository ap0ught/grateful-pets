using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class DropOnDeathEvent : IObjectInventoryInteractionEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(DropOnDeathEvent), null, CountPool, ResetPool);

	private static List<DropOnDeathEvent> Pool;

	private static int PoolCounter;

	public bool WasEquipped;

	public DropOnDeathEvent()
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

	public static void ResetTo(ref DropOnDeathEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static DropOnDeathEvent FromPool()
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
		WasEquipped = false;
	}

	public static DropOnDeathEvent FromPool(GameObject Object, IInventory Inventory, bool WasEquipped = false, bool Forced = true, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = ".", string Type = "DropOnDeath")
	{
		DropOnDeathEvent dropOnDeathEvent = FromPool();
		dropOnDeathEvent.Object = Object;
		dropOnDeathEvent.Inventory = Inventory;
		dropOnDeathEvent.Forced = Forced;
		dropOnDeathEvent.IgnoreGravity = IgnoreGravity;
		dropOnDeathEvent.NoStack = NoStack;
		dropOnDeathEvent.Direction = Direction;
		dropOnDeathEvent.Type = Type;
		dropOnDeathEvent.Dragging = null;
		dropOnDeathEvent.Actor = null;
		dropOnDeathEvent.ForceSwap = null;
		dropOnDeathEvent.Ignore = null;
		dropOnDeathEvent.WasEquipped = WasEquipped;
		return dropOnDeathEvent;
	}

	public static bool Check(GameObject Object, IInventory Inventory, bool WasEquipped = false, bool Forced = true, bool System = false, bool IgnoreGravity = false, bool NoStack = false, string Direction = ".", string Type = "DropOnDeath")
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Inventory, WasEquipped, Forced, System, IgnoreGravity, NoStack: false, Direction, Type)))
		{
			return false;
		}
		return true;
	}
}
