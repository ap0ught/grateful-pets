namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class ExamineSuccessEvent : PooledEvent<ExamineSuccessEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Object;

	public GameObject Actor;

	public bool Complete;

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
		Complete = false;
	}

	public static void Send(GameObject Object, GameObject Actor, bool Complete = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("ExamineSuccess"))
		{
			Event obj = Event.New("ExamineSuccess");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetFlag("Complete", Complete);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("ExamineSuccess"))
		{
			Event obj2 = Event.New("ExamineSuccess");
			obj2.SetParameter("Object", Object);
			obj2.SetParameter("Actor", Actor);
			obj2.SetFlag("Complete", Complete);
			flag = Actor.FireEvent(obj2);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<ExamineSuccessEvent>.ID, CascadeLevel))
		{
			ExamineSuccessEvent examineSuccessEvent = PooledEvent<ExamineSuccessEvent>.FromPool();
			examineSuccessEvent.Object = Object;
			examineSuccessEvent.Actor = Actor;
			examineSuccessEvent.Complete = Complete;
			flag = Object.HandleEvent(examineSuccessEvent);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<ExamineSuccessEvent>.ID, CascadeLevel))
		{
			ExamineSuccessEvent examineSuccessEvent2 = PooledEvent<ExamineSuccessEvent>.FromPool();
			examineSuccessEvent2.Object = Object;
			examineSuccessEvent2.Actor = Actor;
			examineSuccessEvent2.Complete = Complete;
			flag = Actor.HandleEvent(examineSuccessEvent2);
		}
	}
}
