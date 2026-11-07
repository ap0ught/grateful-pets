using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetInventoryActionsAlwaysEvent : IInventoryActionsEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetInventoryActionsAlwaysEvent), null, CountPool, ResetPool);

	private static List<GetInventoryActionsAlwaysEvent> Pool;

	private static int PoolCounter;

	public GetInventoryActionsAlwaysEvent()
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

	public static void ResetTo(ref GetInventoryActionsAlwaysEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetInventoryActionsAlwaysEvent FromPool()
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

	public static GetInventoryActionsAlwaysEvent FromPool(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		GetInventoryActionsAlwaysEvent getInventoryActionsAlwaysEvent = FromPool();
		getInventoryActionsAlwaysEvent.Actor = Actor;
		getInventoryActionsAlwaysEvent.Object = Object;
		getInventoryActionsAlwaysEvent.Actions = Actions;
		return getInventoryActionsAlwaysEvent;
	}

	public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Actor, Object, Actions));
		}
	}
}
