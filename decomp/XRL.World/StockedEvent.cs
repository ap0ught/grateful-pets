namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class StockedEvent : PooledEvent<StockedEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public string Context;

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
		Context = null;
	}

	public static void Send(GameObject Object, string Context = null)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("Stocked"))
		{
			Event obj = Event.New("Stocked");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Context", Context);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<StockedEvent>.ID, CascadeLevel))
		{
			StockedEvent stockedEvent = PooledEvent<StockedEvent>.FromPool();
			stockedEvent.Object = Object;
			stockedEvent.Context = Context;
			flag = Object.HandleEvent(stockedEvent);
		}
	}
}
