namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class EnvironmentalUpdateEvent : PooledEvent<EnvironmentalUpdateEvent>
{
	public new static readonly int CascadeLevel = 15;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public static void Send(GameObject Object)
	{
		if (Object.WantEvent(PooledEvent<EnvironmentalUpdateEvent>.ID, CascadeLevel))
		{
			EnvironmentalUpdateEvent E = PooledEvent<EnvironmentalUpdateEvent>.FromPool();
			Object.HandleEvent(E);
			PooledEvent<EnvironmentalUpdateEvent>.ResetTo(ref E);
		}
	}
}
