namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class AfterReputationChangeEvent : PooledEvent<AfterReputationChangeEvent>
{
	public new static readonly int CascadeLevel = 1;

	public Faction Faction;

	public int From;

	public int To;

	public string Type;

	public bool Silent;

	public bool Transient;

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
		Faction = null;
		From = 0;
		To = 0;
		Type = null;
		Silent = false;
		Transient = false;
	}

	public static void Send(Faction Faction, int From, int To, string Type = null, bool Silent = false, bool Transient = false)
	{
		AfterReputationChangeEvent E = PooledEvent<AfterReputationChangeEvent>.FromPool();
		E.Faction = Faction;
		E.From = From;
		E.To = To;
		E.Type = Type;
		E.Silent = Silent;
		E.Transient = Transient;
		The.Game.HandleEvent(E, DispatchPlayer: true);
		PooledEvent<AfterReputationChangeEvent>.ResetTo(ref E);
	}
}
