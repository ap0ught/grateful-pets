namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetOverloadChargeEvent : PooledEvent<GetOverloadChargeEvent>
{
	public GameObject Object;

	public int Amount;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Amount = 0;
	}

	public static int GetFor(GameObject Object, int Amount)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetOverloadCharge"))
		{
			Event obj = Event.New("GetOverloadCharge");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Amount", Amount);
			flag = Object.FireEvent(obj);
			Amount = obj.GetIntParameter("Amount");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetOverloadChargeEvent>.ID, MinEvent.CascadeLevel))
		{
			GetOverloadChargeEvent getOverloadChargeEvent = PooledEvent<GetOverloadChargeEvent>.FromPool();
			getOverloadChargeEvent.Object = Object;
			getOverloadChargeEvent.Amount = Amount;
			flag = Object.HandleEvent(getOverloadChargeEvent);
			Amount = getOverloadChargeEvent.Amount;
		}
		return Amount;
	}
}
