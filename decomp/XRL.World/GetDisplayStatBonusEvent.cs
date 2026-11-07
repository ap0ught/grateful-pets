namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetDisplayStatBonusEvent : PooledEvent<GetDisplayStatBonusEvent>
{
	public GameObject Object;

	public IComponent<GameObject> Component;

	public string Stat;

	public int BaseAmount;

	public int Amount;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Component = null;
		Stat = null;
		BaseAmount = 0;
		Amount = 0;
	}

	public static int GetFor(GameObject Object, IComponent<GameObject> Component, string Stat, int BaseAmount)
	{
		int num = BaseAmount;
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetDisplayStatBonus"))
		{
			Event obj = Event.New("GetDisplayStatBonus");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Component", Component);
			obj.SetParameter("Stat", Stat);
			obj.SetParameter("BaseAmount", BaseAmount);
			obj.SetParameter("Amount", num);
			flag = Object.FireEvent(obj);
			num = obj.GetIntParameter("Amount");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetDisplayStatBonusEvent>.ID, MinEvent.CascadeLevel))
		{
			GetDisplayStatBonusEvent getDisplayStatBonusEvent = PooledEvent<GetDisplayStatBonusEvent>.FromPool();
			getDisplayStatBonusEvent.Object = Object;
			getDisplayStatBonusEvent.Component = Component;
			getDisplayStatBonusEvent.Stat = Stat;
			getDisplayStatBonusEvent.BaseAmount = BaseAmount;
			getDisplayStatBonusEvent.Amount = num;
			flag = Object.HandleEvent(getDisplayStatBonusEvent);
			num = getDisplayStatBonusEvent.Amount;
		}
		return num;
	}
}
