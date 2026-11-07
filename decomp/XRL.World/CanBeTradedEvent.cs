namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanBeTradedEvent : PooledEvent<CanBeTradedEvent>
{
	public GameObject Object;

	public GameObject Holder;

	public GameObject OtherParty;

	public float CostMultiple;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Holder = null;
		OtherParty = null;
		CostMultiple = 0f;
	}

	public static CanBeTradedEvent FromPool(GameObject Object, GameObject Holder, GameObject OtherParty, float CostMultiple = 1f)
	{
		CanBeTradedEvent canBeTradedEvent = PooledEvent<CanBeTradedEvent>.FromPool();
		canBeTradedEvent.Object = Object;
		canBeTradedEvent.Holder = Holder;
		canBeTradedEvent.OtherParty = OtherParty;
		canBeTradedEvent.CostMultiple = CostMultiple;
		return canBeTradedEvent;
	}

	public static bool Check(GameObject Object, GameObject Holder, GameObject OtherParty, float CostMultiple = 1f)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanBeTradedEvent>.ID, MinEvent.CascadeLevel) && !Object.HandleEvent(FromPool(Object, Holder, OtherParty, CostMultiple)))
		{
			return false;
		}
		return true;
	}
}
