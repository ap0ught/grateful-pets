namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class BeforeAITakingActionEvent : PooledEvent<BeforeAITakingActionEvent>
{
	public new static readonly int CascadeLevel = 17;

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
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("BeforeAITakingAction"))
		{
			Event obj = Event.New("BeforeAITakingAction");
			obj.SetParameter("Actor", Actor);
			flag = Actor.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<BeforeAITakingActionEvent>.ID, CascadeLevel))
		{
			BeforeAITakingActionEvent beforeAITakingActionEvent = PooledEvent<BeforeAITakingActionEvent>.FromPool();
			beforeAITakingActionEvent.Actor = Actor;
			flag = Actor.HandleEvent(beforeAITakingActionEvent);
		}
		return flag;
	}
}
