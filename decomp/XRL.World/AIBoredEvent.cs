namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class AIBoredEvent : PooledEvent<AIBoredEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Actor;

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
	}

	public static bool Check(GameObject Actor)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("AIBored"))
		{
			Event obj = Event.New("AIBored");
			obj.SetParameter("Actor", Actor);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<AIBoredEvent>.ID, CascadeLevel))
		{
			AIBoredEvent aIBoredEvent = PooledEvent<AIBoredEvent>.FromPool();
			aIBoredEvent.Actor = Actor;
			flag = Actor.HandleEvent(aIBoredEvent);
		}
		return flag;
	}
}
