namespace XRL.World;

[GameEvent(Cascade = 271, Cache = Cache.Pool)]
public class ContainsBlueprintEvent : PooledEvent<ContainsBlueprintEvent>
{
	public new static readonly int CascadeLevel = 271;

	public GameObject Container;

	public string Blueprint;

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
		Blueprint = null;
		Object = null;
	}

	public static ContainsBlueprintEvent FromPool(GameObject Container, string Blueprint)
	{
		ContainsBlueprintEvent containsBlueprintEvent = PooledEvent<ContainsBlueprintEvent>.FromPool();
		containsBlueprintEvent.Container = Container;
		containsBlueprintEvent.Blueprint = Blueprint;
		containsBlueprintEvent.Object = null;
		return containsBlueprintEvent;
	}

	public static bool Check(GameObject Container, string Blueprint)
	{
		if (!Container.HandleEvent(FromPool(Container, Blueprint)))
		{
			return true;
		}
		return false;
	}

	public static GameObject Find(GameObject Container, string Blueprint)
	{
		ContainsBlueprintEvent containsBlueprintEvent = FromPool(Container, Blueprint);
		if (!Container.HandleEvent(containsBlueprintEvent))
		{
			return containsBlueprintEvent.Object;
		}
		return null;
	}
}
