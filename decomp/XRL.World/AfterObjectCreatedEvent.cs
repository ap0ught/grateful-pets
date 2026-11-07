using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterObjectCreatedEvent : IObjectCreationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterObjectCreatedEvent), null, CountPool, ResetPool);

	private static List<AfterObjectCreatedEvent> Pool;

	private static int PoolCounter;

	public AfterObjectCreatedEvent()
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

	public static void ResetTo(ref AfterObjectCreatedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterObjectCreatedEvent FromPool()
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
		if (true && Object.HasRegisteredEvent("AfterObjectCreated"))
		{
			Event obj = Event.New("AfterObjectCreated");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Context", Context);
			obj.SetParameter("ReplacementObject", ReplacementObject);
			Object.FireEvent(obj);
			ReplacementObject = obj.GetGameObjectParameter("ReplacementObject");
		}
		if (Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AfterObjectCreatedEvent afterObjectCreatedEvent = FromPool();
			afterObjectCreatedEvent.Object = Object;
			afterObjectCreatedEvent.Context = Context;
			afterObjectCreatedEvent.ReplacementObject = ReplacementObject;
			Object.HandleEvent(afterObjectCreatedEvent);
			ReplacementObject = afterObjectCreatedEvent.ReplacementObject;
		}
	}
}
