using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetCookingActionsEvent : IInventoryActionsEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetCookingActionsEvent), null, CountPool, ResetPool);

	private static List<GetCookingActionsEvent> Pool;

	private static int PoolCounter;

	public GetCookingActionsEvent()
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

	public static void ResetTo(ref GetCookingActionsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetCookingActionsEvent FromPool()
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

	public static GetCookingActionsEvent FromPool(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		GetCookingActionsEvent getCookingActionsEvent = FromPool();
		getCookingActionsEvent.Actor = Actor;
		getCookingActionsEvent.Object = Object;
		getCookingActionsEvent.Actions = Actions;
		return getCookingActionsEvent;
	}

	public static void SendToActorAndObject(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel) || Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetCookingActionsEvent e = FromPool(Actor, Object, Actions);
			Object.HandleEvent(e);
			Actor.HandleEvent(e);
		}
	}

	public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Actor, Object, Actions));
		}
	}
}
