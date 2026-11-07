using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanBeModdedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanBeModdedEvent), null, CountPool, ResetPool);

	private static List<CanBeModdedEvent> Pool;

	private static int PoolCounter;

	public string ModName;

	public CanBeModdedEvent()
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

	public static void ResetTo(ref CanBeModdedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CanBeModdedEvent FromPool()
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
		ModName = null;
	}

	public static bool Check(GameObject Actor, GameObject Item, string ModName)
	{
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("CanBeModded");
			bool flag3 = GameObject.Validate(ref Item) && Item.HasRegisteredEvent("CanBeModded");
			if (flag2 | flag3)
			{
				Event obj = Event.New("CanBeModded");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Item", Item);
				obj.SetParameter("ModName", ModName);
				flag = (!flag2 || Actor.FireEvent(obj)) && (!flag3 || Item.FireEvent(obj));
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.Validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel);
			bool flag5 = GameObject.Validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel);
			if (flag4 | flag5)
			{
				CanBeModdedEvent canBeModdedEvent = FromPool();
				canBeModdedEvent.Actor = Actor;
				canBeModdedEvent.Item = Item;
				canBeModdedEvent.ModName = ModName;
				flag = (!flag4 || Actor.HandleEvent(canBeModdedEvent)) && (!flag5 || Item.HandleEvent(canBeModdedEvent));
			}
		}
		return flag;
	}
}
