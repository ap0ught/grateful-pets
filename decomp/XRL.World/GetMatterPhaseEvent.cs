namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetMatterPhaseEvent : PooledEvent<GetMatterPhaseEvent>
{
	public GameObject Object;

	public int MatterPhase;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		MatterPhase = 0;
	}

	public static GetMatterPhaseEvent FromPool(GameObject Object, int Base)
	{
		GetMatterPhaseEvent getMatterPhaseEvent = PooledEvent<GetMatterPhaseEvent>.FromPool();
		getMatterPhaseEvent.Object = Object;
		getMatterPhaseEvent.MatterPhase = Base;
		return getMatterPhaseEvent;
	}

	public void MinMatterPhase(int MatterPhase)
	{
		if (this.MatterPhase < MatterPhase)
		{
			this.MatterPhase = MatterPhase;
		}
	}

	public static int GetFor(GameObject Object, int Base = 1)
	{
		if (Object != null)
		{
			GetMatterPhaseEvent E = FromPool(Object, Base);
			Object.HandleEvent(E);
			int matterPhase = E.MatterPhase;
			PooledEvent<GetMatterPhaseEvent>.ResetTo(ref E);
			return matterPhase;
		}
		return Base;
	}
}
