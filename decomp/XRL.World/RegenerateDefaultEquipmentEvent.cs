using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class RegenerateDefaultEquipmentEvent : PooledEvent<RegenerateDefaultEquipmentEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public Body Body;

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
		Body = null;
	}

	public static void Send(GameObject Object, Body Body)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("RegenerateDefaultEquipment"))
		{
			Event obj = Event.New("RegenerateDefaultEquipment");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Body", Body);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<RegenerateDefaultEquipmentEvent>.ID, CascadeLevel))
		{
			RegenerateDefaultEquipmentEvent regenerateDefaultEquipmentEvent = PooledEvent<RegenerateDefaultEquipmentEvent>.FromPool();
			regenerateDefaultEquipmentEvent.Object = Object;
			regenerateDefaultEquipmentEvent.Body = Body;
			flag = Object.HandleEvent(regenerateDefaultEquipmentEvent);
		}
	}
}
