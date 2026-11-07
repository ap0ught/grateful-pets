namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterBasicBestowalEvent : PooledEvent<AfterBasicBestowalEvent>
{
	public GameObject Object;

	public string Type;

	public string Subtype;

	public int Tier;

	public bool Standard;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Type = null;
		Subtype = null;
		Tier = 0;
		Standard = false;
	}

	public static void Send(GameObject Object, string Type, string Subtype, int Tier, bool Standard)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AfterBasicBestowal"))
		{
			Event obj = Event.New("AfterBasicBestowal");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Type", Type);
			obj.SetParameter("Subtype", Subtype);
			obj.SetParameter("Tier", Tier);
			obj.SetFlag("Standard", Standard);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<AfterBasicBestowalEvent>.ID, MinEvent.CascadeLevel))
		{
			AfterBasicBestowalEvent afterBasicBestowalEvent = PooledEvent<AfterBasicBestowalEvent>.FromPool();
			afterBasicBestowalEvent.Object = Object;
			afterBasicBestowalEvent.Type = Type;
			afterBasicBestowalEvent.Subtype = Subtype;
			afterBasicBestowalEvent.Tier = Tier;
			afterBasicBestowalEvent.Standard = Standard;
			flag = Object.HandleEvent(afterBasicBestowalEvent);
		}
	}
}
