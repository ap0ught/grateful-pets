namespace XRL.World;

[GameEvent(Cascade = 271, Cache = Cache.Pool)]
public class ContainsEvent : PooledEvent<ContainsEvent>
{
	public new static readonly int CascadeLevel = 271;

	public GameObject Container;

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
		Container = null;
		Object = null;
	}

	public static ContainsEvent FromPool(GameObject Container, GameObject Object)
	{
		ContainsEvent containsEvent = PooledEvent<ContainsEvent>.FromPool();
		containsEvent.Container = Container;
		containsEvent.Object = Object;
		return containsEvent;
	}

	public static bool Check(GameObject Container, GameObject Object)
	{
		if (!Container.HandleEvent(FromPool(Container, Object)))
		{
			return true;
		}
		return false;
	}
}
