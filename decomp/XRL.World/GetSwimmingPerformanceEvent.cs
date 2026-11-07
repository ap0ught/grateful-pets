namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class GetSwimmingPerformanceEvent : PooledEvent<GetSwimmingPerformanceEvent>
{
	public new static readonly int CascadeLevel = 1;

	public GameObject Actor;

	public int MoveSpeedPenalty;

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
		MoveSpeedPenalty = 0;
	}

	public static GetSwimmingPerformanceEvent FromPool(GameObject Actor, int MoveSpeedPenalty)
	{
		GetSwimmingPerformanceEvent getSwimmingPerformanceEvent = PooledEvent<GetSwimmingPerformanceEvent>.FromPool();
		getSwimmingPerformanceEvent.Actor = Actor;
		getSwimmingPerformanceEvent.MoveSpeedPenalty = MoveSpeedPenalty;
		return getSwimmingPerformanceEvent;
	}

	public static bool GetFor(GameObject Actor, ref int MoveSpeedPenalty)
	{
		bool flag = GameObject.Validate(ref Actor);
		if (flag && Actor.HasRegisteredEvent("GetSwimmingPerformance"))
		{
			Event obj = Event.New("GetSwimmingPerformance", "Actor", Actor, "MoveSpeedPenalty", MoveSpeedPenalty);
			flag = Actor.FireEvent(obj);
			MoveSpeedPenalty = obj.GetIntParameter("MoveSpeedPenalty");
		}
		if (flag && Actor.WantEvent(PooledEvent<GetSwimmingPerformanceEvent>.ID, CascadeLevel))
		{
			GetSwimmingPerformanceEvent getSwimmingPerformanceEvent = FromPool(Actor, MoveSpeedPenalty);
			flag = Actor.HandleEvent(getSwimmingPerformanceEvent);
			MoveSpeedPenalty = getSwimmingPerformanceEvent.MoveSpeedPenalty;
		}
		return flag;
	}
}
