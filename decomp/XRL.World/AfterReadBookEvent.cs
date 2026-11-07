namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class AfterReadBookEvent : PooledEvent<AfterReadBookEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Object;

	public object Source;

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
		Object = null;
		Source = null;
	}

	public static void Send(GameObject Actor, GameObject Object, object Source, IEvent ParentEvent = null)
	{
		AfterReadBookEvent E = PooledEvent<AfterReadBookEvent>.FromPool();
		E.Actor = Actor;
		E.Object = Object;
		E.Source = Source;
		if ((Actor == null || Actor.HandleEvent(E)) && Object != null && Object != Actor)
		{
			Object.HandleEvent(E);
		}
		ParentEvent?.ProcessChildEvent(E);
		PooledEvent<AfterReadBookEvent>.ResetTo(ref E);
	}
}
