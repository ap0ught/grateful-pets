namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class AfterPilotChangeEvent : PooledEvent<AfterPilotChangeEvent>
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

	public static void Send(GameObject Vehicle, GameObject NewPilot, GameObject OldPilot)
	{
		AfterPilotChangeEvent E = PooledEvent<AfterPilotChangeEvent>.FromPool();
		E.Vehicle = Vehicle;
		E.NewPilot = NewPilot;
		E.OldPilot = OldPilot;
		Vehicle.HandleEvent(E);
		NewPilot?.HandleEvent(E);
		OldPilot?.HandleEvent(E);
		PooledEvent<AfterPilotChangeEvent>.ResetTo(ref E);
	}
}
