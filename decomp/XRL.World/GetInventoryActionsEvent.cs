using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetInventoryActionsEvent : IInventoryActionsEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetInventoryActionsEvent), null, CountPool, ResetPool);

	private static List<GetInventoryActionsEvent> Pool;

	private static int PoolCounter;

	public GetInventoryActionsEvent()
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

	public static void ResetTo(ref GetInventoryActionsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetInventoryActionsEvent FromPool()
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

	public static GetInventoryActionsEvent FromPool(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		GetInventoryActionsEvent getInventoryActionsEvent = FromPool();
		getInventoryActionsEvent.Actor = Actor;
		getInventoryActionsEvent.Object = Object;
		getInventoryActionsEvent.Actions = Actions;
		return getInventoryActionsEvent;
	}

	public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Actor, Object, Actions));
		}
	}
}
