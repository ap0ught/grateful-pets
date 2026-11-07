using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 17)]
public class ImplantAddedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ImplantAddedEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<ImplantAddedEvent> Pool;

	private static int PoolCounter;

	public GameObject Implantee;

	public BodyPart Part;

	public bool ForDeepCopy;

	public bool Silent;

	public ImplantAddedEvent()
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

	public static void ResetTo(ref ImplantAddedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ImplantAddedEvent FromPool()
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
		if (flag && GameObject.Validate(ref Implantee) && Implantee.HasRegisteredEvent("ImplantAdded"))
		{
			Event obj = new Event("ImplantAdded");
			obj.SetParameter("Implantee", Implantee);
			obj.SetParameter("Actor", Actor ?? Implantee);
			obj.SetParameter("Object", Implantee);
			obj.SetParameter("Implant", Implant);
			obj.SetParameter("BodyPart", Part);
			obj.SetFlag("ForDeepCopy", ForDeepCopy);
			obj.SetFlag("Silent", Silent);
			flag = Implantee.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Implantee) && Implantee.WantEvent(ID, CascadeLevel))
		{
			ImplantAddedEvent implantAddedEvent = FromPool();
			implantAddedEvent.Actor = Actor ?? Implantee;
			implantAddedEvent.Implantee = Implantee;
			implantAddedEvent.Item = Implant;
			implantAddedEvent.Part = Part;
			implantAddedEvent.ForDeepCopy = ForDeepCopy;
			implantAddedEvent.Silent = Silent;
			flag = Implantee.HandleEvent(implantAddedEvent);
		}
	}
}
