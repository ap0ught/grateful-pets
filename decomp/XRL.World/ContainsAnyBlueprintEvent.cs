using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 271, Cache = Cache.Pool)]
public class ContainsAnyBlueprintEvent : PooledEvent<ContainsAnyBlueprintEvent>
{
	public new static readonly int CascadeLevel = 271;

	public GameObject Container;

	public List<string> Blueprints;

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
		Blueprints = null;
		Object = null;
	}

	public static ContainsAnyBlueprintEvent FromPool(GameObject Container, List<string> Blueprints)
	{
		ContainsAnyBlueprintEvent containsAnyBlueprintEvent = PooledEvent<ContainsAnyBlueprintEvent>.FromPool();
		containsAnyBlueprintEvent.Container = Container;
		containsAnyBlueprintEvent.Blueprints = Blueprints;
		containsAnyBlueprintEvent.Object = null;
		return containsAnyBlueprintEvent;
	}

	public static bool Check(GameObject Container, List<string> Blueprints)
	{
		if (!Container.HandleEvent(FromPool(Container, Blueprints)))
		{
			return true;
		}
		return false;
	}

	public static GameObject Find(GameObject Container, List<string> Blueprints)
	{
		ContainsAnyBlueprintEvent containsAnyBlueprintEvent = FromPool(Container, Blueprints);
		if (!Container.HandleEvent(containsAnyBlueprintEvent))
		{
			return containsAnyBlueprintEvent.Object;
		}
		return null;
	}
}
