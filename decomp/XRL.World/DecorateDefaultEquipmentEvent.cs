using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class DecorateDefaultEquipmentEvent : PooledEvent<DecorateDefaultEquipmentEvent>
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
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("DecorateDefaultEquipment"))
		{
			Event obj = Event.New("DecorateDefaultEquipment");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Body", Body);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<DecorateDefaultEquipmentEvent>.ID, CascadeLevel))
		{
			DecorateDefaultEquipmentEvent decorateDefaultEquipmentEvent = PooledEvent<DecorateDefaultEquipmentEvent>.FromPool();
			decorateDefaultEquipmentEvent.Object = Object;
			decorateDefaultEquipmentEvent.Body = Body;
			flag = Object.HandleEvent(decorateDefaultEquipmentEvent);
		}
	}
}
