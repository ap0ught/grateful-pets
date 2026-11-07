using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 273, Cache = Cache.Pool)]
public class OwnerGetInventoryActionsEvent : IInventoryActionsEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(OwnerGetInventoryActionsEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 273;

	private static List<OwnerGetInventoryActionsEvent> Pool;

	private static int PoolCounter;

	public OwnerGetInventoryActionsEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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

	public static void ResetTo(ref OwnerGetInventoryActionsEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static OwnerGetInventoryActionsEvent FromPool()
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

	public static void Send(GameObject Actor, GameObject Object, Dictionary<string, InventoryAction> Actions)
	{
		OwnerGetInventoryActionsEvent E = FromPool();
		E.Actor = Actor;
		E.Object = Object;
		E.Actions = Actions;
		Actor.HandleEvent(E);
		ResetTo(ref E);
	}
}
