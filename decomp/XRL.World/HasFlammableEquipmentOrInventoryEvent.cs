namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class HasFlammableEquipmentOrInventoryEvent : PooledEvent<HasFlammableEquipmentOrInventoryEvent>
{
	public new static readonly int CascadeLevel = 3;

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

	public static HasFlammableEquipmentOrInventoryEvent FromPool(GameObject Object, int Temperature)
	{
		HasFlammableEquipmentOrInventoryEvent hasFlammableEquipmentOrInventoryEvent = PooledEvent<HasFlammableEquipmentOrInventoryEvent>.FromPool();
		hasFlammableEquipmentOrInventoryEvent.Object = Object;
		hasFlammableEquipmentOrInventoryEvent.Temperature = Temperature;
		return hasFlammableEquipmentOrInventoryEvent;
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
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<HasFlammableEquipmentOrInventoryEvent>.ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, Temperature));
		}
		return !flag;
	}
}
