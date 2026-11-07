using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ModificationAppliedEvent : PooledEvent<ModificationAppliedEvent>
{
	public GameObject Object;

	public IModification Modification;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Modification = null;
	}

	public static ModificationAppliedEvent FromPool(GameObject Object, IModification Modification)
	{
		ModificationAppliedEvent modificationAppliedEvent = PooledEvent<ModificationAppliedEvent>.FromPool();
		modificationAppliedEvent.Object = Object;
		modificationAppliedEvent.Modification = Modification;
		return modificationAppliedEvent;
	}

	public static void Send(GameObject Object, IModification Modification)
	{
		if (Object.WantEvent(PooledEvent<ModificationAppliedEvent>.ID, MinEvent.CascadeLevel))
		{
			Object.HandleEvent(FromPool(Object, Modification));
		}
		if (Object.HasRegisteredEvent("ModificationApplied"))
		{
			Object.FireEvent(Event.New("ModificationApplied", "Object", Object, "Modification", Modification));
		}
	}
}
