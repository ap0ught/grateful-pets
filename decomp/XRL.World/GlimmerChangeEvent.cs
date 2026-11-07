namespace XRL.World;

[GameEvent(Cascade = 271, Cache = Cache.Pool)]
public class GlimmerChangeEvent : PooledEvent<GlimmerChangeEvent>
{
	public new static readonly int CascadeLevel = 271;

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
		if (Object.IsPlayer())
		{
			Object.HandleEvent(PooledEvent<GlimmerChangeEvent>.FromPool());
		}
	}
}
