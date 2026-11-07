using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 17)]
public class ImplantRemovedEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ImplantRemovedEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<ImplantRemovedEvent> Pool;

	private static int PoolCounter;

	public GameObject Implantee;

	public BodyPart Part;

	public bool Silent;

	public ImplantRemovedEvent()
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

	public static void ResetTo(ref ImplantRemovedEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ImplantRemovedEvent FromPool()
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
		if (flag && GameObject.Validate(ref Implantee) && Implantee.HasRegisteredEvent("ImplantRemoved"))
		{
			Event obj = new Event("ImplantRemoved");
			obj.SetParameter("Actor", Actor ?? Implantee);
			obj.SetParameter("Implantee", Implantee);
			obj.SetParameter("Object", Implantee);
			obj.SetParameter("Implant", Implant);
			obj.SetParameter("BodyPart", Part);
			obj.SetFlag("Silent", Silent);
			flag = Implantee.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Implantee) && Implantee.WantEvent(ID, CascadeLevel))
		{
			ImplantRemovedEvent implantRemovedEvent = FromPool();
			implantRemovedEvent.Actor = Actor ?? Implantee;
			implantRemovedEvent.Implantee = Implantee;
			implantRemovedEvent.Item = Implant;
			implantRemovedEvent.Part = Part;
			implantRemovedEvent.Silent = Silent;
			flag = Implantee.HandleEvent(implantRemovedEvent);
		}
	}
}
