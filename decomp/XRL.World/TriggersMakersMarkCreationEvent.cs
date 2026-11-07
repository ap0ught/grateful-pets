using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class TriggersMakersMarkCreationEvent : PooledEvent<TriggersMakersMarkCreationEvent>
{
	public GameObject Object;

	public GameObject Actor;

	public IModification ModAdded;

	public string Context;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Actor = null;
		ModAdded = null;
		Context = null;
	}

	public static bool Check(GameObject Object, GameObject Actor, IModification ModAdded = null, string Context = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("TriggersMakersMarkCreation"))
		{
			Event obj = Event.New("TriggersMakersMarkCreation");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("ModAdded", ModAdded);
			obj.SetParameter("Context", Context);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<TriggersMakersMarkCreationEvent>.ID, MinEvent.CascadeLevel))
		{
			TriggersMakersMarkCreationEvent triggersMakersMarkCreationEvent = PooledEvent<TriggersMakersMarkCreationEvent>.FromPool();
			triggersMakersMarkCreationEvent.Object = Object;
			triggersMakersMarkCreationEvent.Actor = Actor;
			triggersMakersMarkCreationEvent.ModAdded = ModAdded;
			triggersMakersMarkCreationEvent.Context = Context;
			flag = Object.HandleEvent(triggersMakersMarkCreationEvent);
		}
		return !flag;
	}
}
