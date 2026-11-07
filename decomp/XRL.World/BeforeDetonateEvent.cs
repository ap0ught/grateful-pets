namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeDetonateEvent : PooledEvent<BeforeDetonateEvent>
{
	public GameObject Object;

	public GameObject Actor;

	public GameObject ApparentTarget;

	public bool Indirect;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Actor = null;
		ApparentTarget = null;
		Indirect = false;
	}

	public static bool Check(GameObject Object, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeforeDetonate"))
		{
			Event obj = Event.New("BeforeDetonate");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("ApparentTarget", ApparentTarget);
			obj.SetFlag("Indirect", Indirect);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<BeforeDetonateEvent>.ID, MinEvent.CascadeLevel))
		{
			BeforeDetonateEvent beforeDetonateEvent = PooledEvent<BeforeDetonateEvent>.FromPool();
			beforeDetonateEvent.Object = Object;
			beforeDetonateEvent.Actor = Actor;
			beforeDetonateEvent.ApparentTarget = ApparentTarget;
			beforeDetonateEvent.Indirect = Indirect;
			flag = Object.HandleEvent(beforeDetonateEvent);
		}
		return flag;
	}
}
