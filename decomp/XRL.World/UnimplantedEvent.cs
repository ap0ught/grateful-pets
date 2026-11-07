using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class UnimplantedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(UnimplantedEvent), null, CountPool, ResetPool);

	private static List<UnimplantedEvent> Pool;

	private static int PoolCounter;

	public GameObject Implantee;

	public BodyPart Part;

	public bool Silent;

	public UnimplantedEvent()
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

	public static void ResetTo(ref UnimplantedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static UnimplantedEvent FromPool()
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
		Implantee = null;
		Part = null;
		Silent = false;
	}

	public static void Send(GameObject Implantee, GameObject Implant, BodyPart Part, GameObject Actor = null, bool Silent = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Implant) && Implant.HasRegisteredEvent("Unimplanted"))
		{
			Event obj = new Event("Unimplanted");
			obj.SetParameter("Actor", Actor ?? Implantee);
			obj.SetParameter("Implantee", Implantee);
			obj.SetParameter("Object", Implantee);
			obj.SetParameter("Implant", Implant);
			obj.SetParameter("BodyPart", Part);
			obj.SetFlag("Silent", Silent);
			flag = Implant.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Implant) && Implant.WantEvent(ID, MinEvent.CascadeLevel))
		{
			UnimplantedEvent unimplantedEvent = FromPool();
			unimplantedEvent.Actor = Actor ?? Implantee;
			unimplantedEvent.Implantee = Implantee;
			unimplantedEvent.Item = Implant;
			unimplantedEvent.Part = Part;
			unimplantedEvent.Silent = Silent;
			flag = Implant.HandleEvent(unimplantedEvent);
		}
	}
}
