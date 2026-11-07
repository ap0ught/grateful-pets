using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AddedToInventoryEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AddedToInventoryEvent), null, CountPool, ResetPool);

	private static List<AddedToInventoryEvent> Pool;

	private static int PoolCounter;

	public bool Silent;

	public bool NoStack;

	public AddedToInventoryEvent()
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

	public static void ResetTo(ref AddedToInventoryEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AddedToInventoryEvent FromPool()
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
		Silent = false;
		NoStack = false;
	}

	public static void Send(GameObject Actor, GameObject Item, bool Silent = false, bool NoStack = false, IEvent ParentEvent = null)
	{
		bool flag = true;
		if (GameObject.Validate(ref Item) && Item.HasRegisteredEvent("AddedToInventory"))
		{
			Event obj = Event.New("AddedToInventory");
			obj.SetParameter("TakingObject", Actor);
			obj.SetParameter("Object", Item);
			obj.SetFlag("NoStack", NoStack);
			obj.SetSilent(Silent);
			flag = Item.FireEvent(obj);
			ParentEvent?.ProcessChildEvent(obj);
		}
		if (flag && GameObject.Validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AddedToInventoryEvent addedToInventoryEvent = FromPool();
			addedToInventoryEvent.Actor = Actor;
			addedToInventoryEvent.Item = Item;
			addedToInventoryEvent.Silent = Silent;
			addedToInventoryEvent.NoStack = NoStack;
			flag = Item.HandleEvent(addedToInventoryEvent);
			ParentEvent?.ProcessChildEvent(addedToInventoryEvent);
		}
	}
}
