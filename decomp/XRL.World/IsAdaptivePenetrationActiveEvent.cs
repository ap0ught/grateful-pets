namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class IsAdaptivePenetrationActiveEvent : PooledEvent<IsAdaptivePenetrationActiveEvent>
{
	public GameObject Object;

	public int Bonus;

	public string Symbol;

	public bool Active;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Bonus = 0;
		Symbol = null;
		Active = false;
	}

	public static bool Check(GameObject Object, ref int Bonus)
	{
		string Symbol = "";
		return Check(Object, ref Bonus, ref Symbol);
	}

	public static bool Check(GameObject Object, ref int Bonus, ref string Symbol)
	{
		bool flag = true;
		bool flag2 = false;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("IsAdaptivePenetrationActive"))
		{
			Event obj = Event.New("IsAdaptivePenetrationActive");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Bonus", Bonus);
			obj.SetParameter("Symbol", Symbol);
			obj.SetParameter("Active", flag2);
			flag = Object.FireEvent(obj);
			Bonus = obj.GetIntParameter("Bonus");
			Symbol = obj.GetStringParameter("Symbol");
			flag2 = obj.HasFlag("Active");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<IsAdaptivePenetrationActiveEvent>.ID, MinEvent.CascadeLevel))
		{
			IsAdaptivePenetrationActiveEvent isAdaptivePenetrationActiveEvent = PooledEvent<IsAdaptivePenetrationActiveEvent>.FromPool();
			isAdaptivePenetrationActiveEvent.Object = Object;
			isAdaptivePenetrationActiveEvent.Bonus = Bonus;
			isAdaptivePenetrationActiveEvent.Symbol = Symbol;
			isAdaptivePenetrationActiveEvent.Active = flag2;
			flag = Object.HandleEvent(isAdaptivePenetrationActiveEvent);
			Bonus = isAdaptivePenetrationActiveEvent.Bonus;
			Symbol = isAdaptivePenetrationActiveEvent.Symbol;
			flag2 = isAdaptivePenetrationActiveEvent.Active;
		}
		return flag2;
	}
}
