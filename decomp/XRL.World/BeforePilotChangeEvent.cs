namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class BeforePilotChangeEvent : PooledEvent<BeforePilotChangeEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Vehicle;

	public GameObject NewPilot;

	public GameObject OldPilot;

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
		Vehicle = null;
		NewPilot = null;
		OldPilot = null;
	}

	public static bool Check(GameObject Vehicle, GameObject NewPilot, GameObject OldPilot)
	{
		BeforePilotChangeEvent E = PooledEvent<BeforePilotChangeEvent>.FromPool();
		E.Vehicle = Vehicle;
		E.NewPilot = NewPilot;
		E.OldPilot = OldPilot;
		try
		{
			if (!Vehicle.HandleEvent(E))
			{
				return false;
			}
			if (NewPilot != null && !NewPilot.HandleEvent(E))
			{
				return false;
			}
			if (OldPilot != null && !OldPilot.HandleEvent(E))
			{
				return false;
			}
			return true;
		}
		finally
		{
			PooledEvent<BeforePilotChangeEvent>.ResetTo(ref E);
		}
	}
}
