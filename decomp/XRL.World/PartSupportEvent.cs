namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class PartSupportEvent : PooledEvent<PartSupportEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public string Type;

	public IPart Part;

	public IPart Skip;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Type = null;
		Part = null;
		Skip = null;
	}

	public static PartSupportEvent FromPool(GameObject Actor, string Type, IPart Part, IPart Skip = null)
	{
		PartSupportEvent partSupportEvent = PooledEvent<PartSupportEvent>.FromPool();
		partSupportEvent.Actor = Actor;
		partSupportEvent.Type = Type;
		partSupportEvent.Part = Part;
		partSupportEvent.Skip = Skip;
		return partSupportEvent;
	}

	public static bool Check(GameObject Actor, string Type, IPart Part, IPart Skip = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("PartSupport"))
		{
			Event obj = Event.New("PartSupport");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Type", Type);
			obj.SetParameter("Part", Part);
			obj.SetParameter("Skip", Skip);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<PartSupportEvent>.ID, CascadeLevel))
		{
			flag = Actor.HandleEvent(FromPool(Actor, Type, Part, Skip));
		}
		return !flag;
	}

	public static bool Check(NeedPartSupportEvent PE, IPart Part)
	{
		return Check(PE.Actor, PE.Type, Part, PE.Skip);
	}
}
