namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class NeedPartSupportEvent : PooledEvent<NeedPartSupportEvent>
{
	public GameObject Actor;

	public string Type;

	public IPart Skip;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Type = null;
		Skip = null;
	}

	public static NeedPartSupportEvent FromPool(GameObject Actor, string Type, IPart Skip = null)
	{
		NeedPartSupportEvent needPartSupportEvent = PooledEvent<NeedPartSupportEvent>.FromPool();
		needPartSupportEvent.Actor = Actor;
		needPartSupportEvent.Type = Type;
		needPartSupportEvent.Skip = Skip;
		return needPartSupportEvent;
	}

	public static void Send(GameObject Actor, string Type, IPart Skip = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(Actor) && Actor.HasRegisteredEvent("NeedPartSupport"))
		{
			Event obj = Event.New("NeedPartSupport");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Type", Type);
			obj.SetParameter("Skip", Skip);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<NeedPartSupportEvent>.ID, MinEvent.CascadeLevel))
		{
			flag = Actor.HandleEvent(FromPool(Actor, Type, Skip));
		}
	}
}
