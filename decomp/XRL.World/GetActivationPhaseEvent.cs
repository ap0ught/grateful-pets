namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetActivationPhaseEvent : PooledEvent<GetActivationPhaseEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public int Phase;

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
		Phase = 0;
	}

	public static int GetFor(GameObject Object, int Phase = 0)
	{
		if (Phase == 0 && GameObject.Validate(ref Object))
		{
			Phase = Object.GetPhase();
		}
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetActivationPhase"))
		{
			Event obj = Event.New("GetActivationPhase");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Phase", Phase);
			flag = Object.FireEvent(obj);
			Phase = obj.GetIntParameter("Phase");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetActivationPhaseEvent>.ID, CascadeLevel))
		{
			GetActivationPhaseEvent getActivationPhaseEvent = PooledEvent<GetActivationPhaseEvent>.FromPool();
			getActivationPhaseEvent.Object = Object;
			getActivationPhaseEvent.Phase = Phase;
			flag = Object.HandleEvent(getActivationPhaseEvent);
			Phase = getActivationPhaseEvent.Phase;
		}
		return Phase;
	}
}
