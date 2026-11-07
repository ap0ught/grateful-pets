namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class AIAfterThrowEvent : PooledEvent<AIAfterThrowEvent>
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
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AIAfterThrow"))
		{
			Event obj = Event.New("AIAfterThrow");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIAfterThrow"))
		{
			Event obj2 = Event.New("AIAfterThrow");
			obj2.SetParameter("Object", Object);
			obj2.SetParameter("Actor", Actor);
			obj2.SetParameter("Target", Target);
			flag = Actor.FireEvent(obj2);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<AIAfterThrowEvent>.ID, CascadeLevel))
		{
			AIAfterThrowEvent aIAfterThrowEvent = PooledEvent<AIAfterThrowEvent>.FromPool();
			aIAfterThrowEvent.Object = Object;
			aIAfterThrowEvent.Actor = Actor;
			aIAfterThrowEvent.Target = Target;
			flag = Actor.HandleEvent(aIAfterThrowEvent);
		}
	}
}
