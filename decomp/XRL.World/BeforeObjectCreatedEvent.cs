using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeObjectCreatedEvent : IObjectCreationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeObjectCreatedEvent), null, CountPool, ResetPool);

	private static List<BeforeObjectCreatedEvent> Pool;

	private static int PoolCounter;

	public BeforeObjectCreatedEvent()
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

	public static void ResetTo(ref BeforeObjectCreatedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeObjectCreatedEvent FromPool()
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
		if (true && Object.HasRegisteredEvent("BeforeObjectCreated"))
		{
			Event obj = Event.New("BeforeObjectCreated");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Context", Context);
			obj.SetParameter("ReplacementObject", ReplacementObject);
			Object.FireEvent(obj);
			ReplacementObject = obj.GetGameObjectParameter("ReplacementObject");
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			BeforeObjectCreatedEvent beforeObjectCreatedEvent = FromPool();
			beforeObjectCreatedEvent.Object = Object;
			beforeObjectCreatedEvent.Context = Context;
			beforeObjectCreatedEvent.ReplacementObject = ReplacementObject;
			Object.HandleEvent(beforeObjectCreatedEvent);
			ReplacementObject = beforeObjectCreatedEvent.ReplacementObject;
		}
	}
}
