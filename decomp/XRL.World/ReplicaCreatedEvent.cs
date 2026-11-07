using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class ReplicaCreatedEvent : IReplicationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ReplicaCreatedEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<ReplicaCreatedEvent> Pool;

	private static int PoolCounter;

	public GameObject Original;

	public List<IPart> PartsToRemove;

	public ReplicaCreatedEvent()
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

	public static void ResetTo(ref ReplicaCreatedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ReplicaCreatedEvent FromPool()
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
		Original = null;
		PartsToRemove = null;
	}

	public void WantToRemove(IPart Part)
	{
		if (PartsToRemove != null)
		{
			if (!PartsToRemove.Contains(Part))
			{
				PartsToRemove.Add(Part);
			}
		}
		else
		{
			Object.RemovePart(Part);
		}
	}

	public static void Send(GameObject Object, GameObject Actor, GameObject Original, string Context = null, bool Temporary = false)
	{
		List<IPart> list = null;
		try
		{
			if (GameObject.Validate(ref Object) && Object.HasRegisteredEvent("ReplicaCreated"))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				Event obj = Event.New("ReplicaCreated");
				obj.SetParameter("Object", Object);
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Context", Context);
				obj.SetParameter("Temporary", Temporary ? 1 : 0);
				obj.SetParameter("Original", Original);
				obj.SetParameter("PartsToRemove", list);
				Object.FireEvent(obj);
			}
			if (GameObject.Validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				ReplicaCreatedEvent replicaCreatedEvent = FromPool();
				replicaCreatedEvent.Object = Object;
				replicaCreatedEvent.Actor = Actor;
				replicaCreatedEvent.Original = Original;
				replicaCreatedEvent.Context = Context;
				replicaCreatedEvent.Temporary = Temporary;
				replicaCreatedEvent.PartsToRemove = list;
				Object.HandleEvent(replicaCreatedEvent);
			}
		}
		finally
		{
			if (list != null)
			{
				foreach (IPart item in list)
				{
					try
					{
						Object.RemovePart(item);
					}
					catch (Exception message)
					{
						MetricsManager.LogError(message);
					}
				}
			}
		}
	}
}
