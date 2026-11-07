namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class HasFlammableEquipmentEvent : PooledEvent<HasFlammableEquipmentEvent>
{
	public new static readonly int CascadeLevel = 1;

	public GameObject Object;

	public int Temperature;

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
		Temperature = 0;
	}

	public static HasFlammableEquipmentEvent FromPool(GameObject Object, int Temperature)
	{
		HasFlammableEquipmentEvent hasFlammableEquipmentEvent = PooledEvent<HasFlammableEquipmentEvent>.FromPool();
		hasFlammableEquipmentEvent.Object = Object;
		hasFlammableEquipmentEvent.Temperature = Temperature;
		return hasFlammableEquipmentEvent;
	}

	public static bool Check(GameObject Object, int Temperature)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("HasFlammableEquipment"))
		{
			Event obj = Event.New("HasFlammableEquipment");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Temperature", Temperature);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<HasFlammableEquipmentEvent>.ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, Temperature));
		}
		return !flag;
	}
}
