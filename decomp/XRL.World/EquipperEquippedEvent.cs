using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class EquipperEquippedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(EquipperEquippedEvent), null, CountPool, ResetPool);

	private static List<EquipperEquippedEvent> Pool;

	private static int PoolCounter;

	public BodyPart Part;

	public EquipperEquippedEvent()
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

	public static void ResetTo(ref EquipperEquippedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static EquipperEquippedEvent FromPool()
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

	public static EquipperEquippedEvent FromPool(GameObject Actor, GameObject Item, BodyPart Part)
	{
		EquipperEquippedEvent equipperEquippedEvent = FromPool();
		equipperEquippedEvent.Actor = Actor;
		equipperEquippedEvent.Item = Item;
		equipperEquippedEvent.Part = Part;
		return equipperEquippedEvent;
	}

	public static void Send(GameObject Actor, GameObject Item, BodyPart Part)
	{
		if ((!GameObject.Validate(ref Actor) || !Actor.WantEvent(ID, MinEvent.CascadeLevel) || Actor.HandleEvent(FromPool(Actor, Item, Part))) && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("EquipperEquipped"))
		{
			Event obj = new Event("EquipperEquipped");
			obj.SetParameter("Object", Item);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("BodyPart", Part);
			Actor.FireEvent(obj);
		}
	}
}
