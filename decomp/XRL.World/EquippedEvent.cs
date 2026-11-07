using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EquippedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EquippedEvent), null, CountPool, ResetPool);

	private static List<EquippedEvent> Pool;

	private static int PoolCounter;

	public BodyPart Part;

	public EquippedEvent()
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

	public static void ResetTo(ref EquippedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EquippedEvent FromPool()
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
		Part = null;
	}

	public static void Send(GameObject Actor, GameObject Item, BodyPart Part)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Item) && Item.HasRegisteredEvent("Equipped"))
		{
			Event obj = new Event("Equipped");
			obj.SetParameter("EquippingObject", Actor);
			obj.SetParameter("Object", Item);
			obj.SetParameter("BodyPart", Part);
			obj.SetParameter("Actor", Actor);
			flag = Item.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Item) && Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			EquippedEvent equippedEvent = FromPool();
			equippedEvent.Actor = Actor;
			equippedEvent.Item = Item;
			equippedEvent.Part = Part;
			flag = Item.HandleEvent(equippedEvent);
		}
	}
}
