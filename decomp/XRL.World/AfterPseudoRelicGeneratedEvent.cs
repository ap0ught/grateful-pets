namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterPseudoRelicGeneratedEvent : PooledEvent<AfterPseudoRelicGeneratedEvent>
{
	public GameObject Object;

	public string Element;

	public string Type;

	public string Subtype;

	public int Tier;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Element = null;
		Type = null;
		Subtype = null;
		Tier = 0;
	}

	public static void Send(GameObject Object, string Element, string Type, string Subtype, int Tier)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("AfterPseudoRelicGenerated"))
		{
			Event obj = Event.New("AfterPseudoRelicGenerated");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Element", Element);
			obj.SetParameter("Type", Type);
			obj.SetParameter("Subtype", Subtype);
			obj.SetParameter("Tier", Tier);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<AfterPseudoRelicGeneratedEvent>.ID, MinEvent.CascadeLevel))
		{
			AfterPseudoRelicGeneratedEvent afterPseudoRelicGeneratedEvent = PooledEvent<AfterPseudoRelicGeneratedEvent>.FromPool();
			afterPseudoRelicGeneratedEvent.Object = Object;
			afterPseudoRelicGeneratedEvent.Element = Element;
			afterPseudoRelicGeneratedEvent.Type = Type;
			afterPseudoRelicGeneratedEvent.Subtype = Subtype;
			afterPseudoRelicGeneratedEvent.Tier = Tier;
			flag = Object.HandleEvent(afterPseudoRelicGeneratedEvent);
		}
	}
}
