using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ImplantedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ImplantedEvent), null, CountPool, ResetPool);

	private static List<ImplantedEvent> Pool;

	private static int PoolCounter;

	public GameObject Implantee;

	public BodyPart Part;

	public bool ForDeepCopy;

	public bool Silent;

	public ImplantedEvent()
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

	public static void ResetTo(ref ImplantedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ImplantedEvent FromPool()
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
		ForDeepCopy = false;
		Silent = false;
	}

	public static void Send(GameObject Implantee, GameObject Implant, BodyPart Part, GameObject Actor = null, bool ForDeepCopy = false, bool Silent = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Implant) && Implant.HasRegisteredEvent("Implanted"))
		{
			Event obj = new Event("Implanted");
			obj.SetParameter("Implantee", Implantee);
			obj.SetParameter("Actor", Actor ?? Implantee);
			obj.SetParameter("Object", Implantee);
			obj.SetParameter("Implant", Implant);
			obj.SetParameter("BodyPart", Part);
			obj.SetFlag("ForDeepCopy", ForDeepCopy);
			obj.SetFlag("Silent", Silent);
			flag = Implant.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Implant) && Implant.WantEvent(ID, MinEvent.CascadeLevel))
		{
			ImplantedEvent implantedEvent = FromPool();
			implantedEvent.Actor = Actor ?? Implantee;
			implantedEvent.Implantee = Implantee;
			implantedEvent.Item = Implant;
			implantedEvent.Part = Part;
			implantedEvent.ForDeepCopy = ForDeepCopy;
			implantedEvent.Silent = Silent;
			flag = Implant.HandleEvent(implantedEvent);
		}
	}

	public static void Send(GameObject Implantee, GameObject Implant, IPart SpecificPart, BodyPart Part, GameObject Actor = null, bool ForDeepCopy = false, bool Silent = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Implant) && Implant.HasRegisteredEventFrom("Implanted", SpecificPart))
		{
			Event obj = new Event("Implanted");
			obj.SetParameter("Implantee", Implantee);
			obj.SetParameter("Actor", Actor ?? Implantee);
			obj.SetParameter("Object", Implantee);
			obj.SetParameter("Implant", Implant);
			obj.SetParameter("BodyPart", Part);
			obj.SetFlag("ForDeepCopy", ForDeepCopy);
			obj.SetFlag("Silent", Silent);
			flag = SpecificPart.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Implant) && SpecificPart.WantEvent(ID, MinEvent.CascadeLevel))
		{
			ImplantedEvent implantedEvent = FromPool();
			implantedEvent.Actor = Actor ?? Implantee;
			implantedEvent.Implantee = Implantee;
			implantedEvent.Item = Implant;
			implantedEvent.Part = Part;
			implantedEvent.ForDeepCopy = ForDeepCopy;
			implantedEvent.Silent = Silent;
			flag = SpecificPart.HandleEvent(implantedEvent);
		}
	}
}
