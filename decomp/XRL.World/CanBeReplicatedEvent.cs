using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanBeReplicatedEvent : IReplicationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(CanBeReplicatedEvent), null, CountPool, ResetPool);

	private static List<CanBeReplicatedEvent> Pool;

	private static int PoolCounter;

	public CanBeReplicatedEvent()
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

	public static void ResetTo(ref CanBeReplicatedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static CanBeReplicatedEvent FromPool()
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

	public static bool Check(GameObject Object, GameObject Actor, string Context = null, bool Temporary = false)
	{
		bool flag = true;
		if (GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanBeReplicated"))
		{
			Event obj = Event.New("CanBeReplicated");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Context", Context);
			obj.SetParameter("Temporary", Temporary ? 1 : 0);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			CanBeReplicatedEvent canBeReplicatedEvent = FromPool();
			canBeReplicatedEvent.Object = Object;
			canBeReplicatedEvent.Actor = Actor;
			canBeReplicatedEvent.Context = Context;
			canBeReplicatedEvent.Temporary = Temporary;
			flag = Object.HandleEvent(canBeReplicatedEvent);
		}
		return flag;
	}
}
