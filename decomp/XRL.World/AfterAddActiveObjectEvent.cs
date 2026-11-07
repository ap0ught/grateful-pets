namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Pool)]
public class AfterAddActiveObjectEvent : PooledEvent<AfterAddActiveObjectEvent>
{
	public new static readonly int CascadeLevel;

	public GameObject Object;

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
	}

	public static void Send(GameObject Object)
	{
		AfterAddActiveObjectEvent E = PooledEvent<AfterAddActiveObjectEvent>.FromPool();
		E.Object = Object;
		The.Game.HandleEvent(E);
		Object.HandleEvent(E);
		PooledEvent<AfterAddActiveObjectEvent>.ResetTo(ref E);
	}
}
