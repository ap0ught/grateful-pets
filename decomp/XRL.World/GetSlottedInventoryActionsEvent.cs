using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetSlottedInventoryActionsEvent : IInventoryActionsEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetSlottedInventoryActionsEvent), null, CountPool, ResetPool);

	private static List<GetSlottedInventoryActionsEvent> Pool;

	private static int PoolCounter;

	public GameObject Slotted;

	public GetSlottedInventoryActionsEvent()
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

	public static void ResetTo(ref GetSlottedInventoryActionsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetSlottedInventoryActionsEvent FromPool()
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
		Slotted = null;
	}

	public static GetSlottedInventoryActionsEvent FromPool(GameObject Slotted, GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		GetSlottedInventoryActionsEvent getSlottedInventoryActionsEvent = FromPool();
		getSlottedInventoryActionsEvent.Actor = Actor;
		getSlottedInventoryActionsEvent.Object = Object;
		getSlottedInventoryActionsEvent.Actions = Actions;
		return getSlottedInventoryActionsEvent;
	}

	public static GetSlottedInventoryActionsEvent FromPool(GameObject Slotted, IInventoryActionsEvent Parent)
	{
		return FromPool(Slotted, Parent.Actor, Parent.Object, Parent.Actions);
	}

	public static void Send(GameObject Slotted, IInventoryActionsEvent Parent)
	{
		if (GameObject.Validate(ref Slotted) && Slotted.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetSlottedInventoryActionsEvent e = FromPool(Slotted, Parent);
			Slotted.HandleEvent(e);
			Parent.ProcessChildEvent(e);
		}
	}
}
