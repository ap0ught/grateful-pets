using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class PowerSwitchFlippedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(PowerSwitchFlippedEvent), null, CountPool, ResetPool);

	private static List<PowerSwitchFlippedEvent> Pool;

	private static int PoolCounter;

	public PowerSwitchFlippedEvent()
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

	public static void ResetTo(ref PowerSwitchFlippedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static PowerSwitchFlippedEvent FromPool()
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

	public static void Send(GameObject Actor, GameObject Item)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Item) && Item.HasRegisteredEvent("PowerSwitchFlipped"))
		{
			Event obj = Event.New("PowerSwitchFlipped");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Item", Item);
			flag = Item.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			PowerSwitchFlippedEvent powerSwitchFlippedEvent = FromPool();
			powerSwitchFlippedEvent.Actor = Actor;
			powerSwitchFlippedEvent.Item = Item;
			flag = Item.HandleEvent(powerSwitchFlippedEvent);
		}
	}
}
