namespace XRL.World;

[GameEvent(Cache = Cache.Singleton)]
public class UseEnergyEvent : SingletonEvent<UseEnergyEvent>
{
	public GameObject Actor;

	public int Amount;

	public string Type;

	public bool Passive;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Amount = 0;
		Type = null;
		Passive = false;
	}

	public static void Send(GameObject Actor, int Amount, string Type, bool Passive = false)
	{
		if (Actor.WantEvent(SingletonEvent<UseEnergyEvent>.ID, MinEvent.CascadeLevel))
		{
			SingletonEvent<UseEnergyEvent>.Instance.Actor = Actor;
			SingletonEvent<UseEnergyEvent>.Instance.Amount = Amount;
			SingletonEvent<UseEnergyEvent>.Instance.Type = Type;
			SingletonEvent<UseEnergyEvent>.Instance.Passive = Passive;
			Actor.HandleEvent(SingletonEvent<UseEnergyEvent>.Instance);
			SingletonEvent<UseEnergyEvent>.Instance.Reset();
		}
	}
}
