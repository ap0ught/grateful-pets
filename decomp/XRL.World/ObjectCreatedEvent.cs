using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ObjectCreatedEvent : IObjectCreationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ObjectCreatedEvent), null, CountPool, ResetPool);

	private static List<ObjectCreatedEvent> Pool;

	private static int PoolCounter;

	public ObjectCreatedEvent()
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

	public static void ResetTo(ref ObjectCreatedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ObjectCreatedEvent FromPool()
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

	public static void Process(GameObject Object, string Context, ref GameObject ReplacementObject)
	{
		if (true && Object.HasRegisteredEvent("ObjectCreated"))
		{
			Event obj = Event.New("ObjectCreated");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Context", Context);
			obj.SetParameter("ReplacementObject", ReplacementObject);
			Object.FireEvent(obj);
			ReplacementObject = obj.GetGameObjectParameter("ReplacementObject");
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			ObjectCreatedEvent objectCreatedEvent = FromPool();
			objectCreatedEvent.Object = Object;
			objectCreatedEvent.Context = Context;
			objectCreatedEvent.ReplacementObject = ReplacementObject;
			Object.HandleEvent(objectCreatedEvent);
			ReplacementObject = objectCreatedEvent.ReplacementObject;
		}
	}
}
