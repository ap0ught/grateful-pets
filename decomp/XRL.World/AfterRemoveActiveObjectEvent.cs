namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Pool)]
public class AfterRemoveActiveObjectEvent : PooledEvent<AfterRemoveActiveObjectEvent>
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
		if (GameObject.Validate(Object))
		{
			AfterRemoveActiveObjectEvent E = PooledEvent<AfterRemoveActiveObjectEvent>.FromPool();
			E.Object = Object;
			The.Game.HandleEvent(E);
			Object.HandleEvent(E);
			PooledEvent<AfterRemoveActiveObjectEvent>.ResetTo(ref E);
		}
	}
}
