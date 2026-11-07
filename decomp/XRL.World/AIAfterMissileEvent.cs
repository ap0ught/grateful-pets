namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class AIAfterMissileEvent : PooledEvent<AIAfterMissileEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public GameObject Actor;

	public GameObject Target;

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
		Object = null;
		Actor = null;
		Target = null;
	}

	public static void Send(GameObject Object, GameObject Actor, GameObject Target)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AIAfterMissile"))
		{
			Event obj = Event.New("AIAfterMissile");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Actor", Actor);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIAfterMissile"))
		{
			Event obj2 = Event.New("AIAfterMissile");
			obj2.SetParameter("Object", Object);
			obj2.SetParameter("Actor", Actor);
			obj2.SetParameter("Target", Target);
			flag = Actor.FireEvent(obj2);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<AIAfterMissileEvent>.ID, CascadeLevel))
		{
			AIAfterMissileEvent aIAfterMissileEvent = PooledEvent<AIAfterMissileEvent>.FromPool();
			aIAfterMissileEvent.Object = Object;
			aIAfterMissileEvent.Actor = Actor;
			aIAfterMissileEvent.Target = Target;
			flag = Actor.HandleEvent(aIAfterMissileEvent);
		}
	}
}
