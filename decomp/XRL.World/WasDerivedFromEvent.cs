using System;
using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class WasDerivedFromEvent : IDerivationEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(WasDerivedFromEvent), null, CountPool, ResetPool);

	private static List<WasDerivedFromEvent> Pool;

	private static int PoolCounter;

	public GameObject Derivation;

	public List<GameObject> Additional;

	public List<GameObject> All;

	public WasDerivedFromEvent()
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

	public static void ResetTo(ref WasDerivedFromEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static WasDerivedFromEvent FromPool()
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
		Derivation = null;
		Additional = null;
		All = null;
	}

	public static void Send(GameObject Object, GameObject Actor, GameObject Derivation, List<GameObject> Additional, List<GameObject> All, string Context = null)
	{
		if (GameObject.Validate(ref Object) && Object.HasRegisteredEvent("WasDerivedFrom"))
		{
			Event obj = Event.New("WasDerivedFrom");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Context", Context);
			obj.SetParameter("Derivation", Derivation);
			obj.SetParameter("Additional", Additional);
			obj.SetParameter("Additional", All);
			Object.FireEvent(obj);
		}
		if (GameObject.Validate(ref Object) && Object.WantEvent(ID, MinEvent.CascadeLevel))
		{
			WasDerivedFromEvent wasDerivedFromEvent = FromPool();
			wasDerivedFromEvent.Object = Object;
			wasDerivedFromEvent.Actor = Actor;
			wasDerivedFromEvent.Derivation = Derivation;
			wasDerivedFromEvent.Additional = Additional;
			wasDerivedFromEvent.All = All;
			wasDerivedFromEvent.Context = Context;
			Object.HandleEvent(wasDerivedFromEvent);
		}
	}

	public void ApplyToEach(Action<GameObject> Func)
	{
		if (All != null)
		{
			foreach (GameObject item in All)
			{
				Func(item);
			}
			return;
		}
		Func(Derivation);
		if (Additional == null)
		{
			return;
		}
		foreach (GameObject item2 in Additional)
		{
			Func(item2);
		}
	}
}
