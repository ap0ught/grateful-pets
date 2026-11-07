namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Singleton)]
public class StartTradeEvent : SingletonEvent<StartTradeEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Trader;

	public int IdentifyLevel;

	public bool Companion;

	public bool Identify;

	public bool Repair;

	public bool Recharge;

	public bool Read;

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
		Actor = null;
		Trader = null;
		IdentifyLevel = 0;
		Companion = false;
		Identify = false;
		Repair = false;
		Recharge = false;
		Read = false;
	}

	public static void Send(GameObject Actor, GameObject Trader, int IdentifyLevel = 0, bool Companion = false, bool Identify = false, bool Repair = false, bool Recharge = false, bool Read = false)
	{
		SingletonEvent<StartTradeEvent>.Instance.Actor = Actor;
		SingletonEvent<StartTradeEvent>.Instance.Trader = Trader;
		SingletonEvent<StartTradeEvent>.Instance.IdentifyLevel = IdentifyLevel;
		SingletonEvent<StartTradeEvent>.Instance.Companion = Companion;
		SingletonEvent<StartTradeEvent>.Instance.Identify = Identify;
		SingletonEvent<StartTradeEvent>.Instance.Repair = Repair;
		SingletonEvent<StartTradeEvent>.Instance.Recharge = Recharge;
		SingletonEvent<StartTradeEvent>.Instance.Read = Read;
		if (Actor.HandleEvent(SingletonEvent<StartTradeEvent>.Instance))
		{
			Trader.HandleEvent(SingletonEvent<StartTradeEvent>.Instance);
		}
		SingletonEvent<StartTradeEvent>.Instance.Reset();
	}
}
