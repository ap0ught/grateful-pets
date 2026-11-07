using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class DerivationCreatedEvent : IDerivationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(DerivationCreatedEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<DerivationCreatedEvent> Pool;

	private static int PoolCounter;

	public GameObject Original;

	public List<IPart> PartsToRemove;

	public DerivationCreatedEvent()
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

	public static void ResetTo(ref DerivationCreatedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static DerivationCreatedEvent FromPool()
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

	public static void Send(GameObject Object, GameObject Actor, GameObject Original, string Context = null)
	{
		List<IPart> list = null;
		try
		{
			if (GameObject.Validate(ref Object) && Object.HasRegisteredEvent("DerivationCreated"))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				Event obj = Event.New("DerivationCreated");
				obj.SetParameter("Object", Object);
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Context", Context);
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
				DerivationCreatedEvent derivationCreatedEvent = FromPool();
				derivationCreatedEvent.Object = Object;
				derivationCreatedEvent.Actor = Actor;
				derivationCreatedEvent.Original = Original;
				derivationCreatedEvent.Context = Context;
				derivationCreatedEvent.PartsToRemove = list;
				Object.HandleEvent(derivationCreatedEvent);
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
