namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetRandomBuyChimericBodyPartRollsEvent : PooledEvent<GetRandomBuyChimericBodyPartRollsEvent>
{
	public GameObject Actor;

	public int BaseAmount;

	public int Amount;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		BaseAmount = 0;
		Amount = 0;
	}

	public static GetRandomBuyChimericBodyPartRollsEvent FromPool(GameObject Actor, int BaseAmount, int Amount)
	{
		GetRandomBuyChimericBodyPartRollsEvent getRandomBuyChimericBodyPartRollsEvent = PooledEvent<GetRandomBuyChimericBodyPartRollsEvent>.FromPool();
		getRandomBuyChimericBodyPartRollsEvent.Actor = Actor;
		getRandomBuyChimericBodyPartRollsEvent.BaseAmount = BaseAmount;
		getRandomBuyChimericBodyPartRollsEvent.Amount = Amount;
		return getRandomBuyChimericBodyPartRollsEvent;
	}

	public static int GetFor(GameObject Actor, int BaseAmount)
	{
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetRandomBuyChimericBodyPartRollsEvent>.ID, MinEvent.CascadeLevel))
		{
			GetRandomBuyChimericBodyPartRollsEvent getRandomBuyChimericBodyPartRollsEvent = FromPool(Actor, BaseAmount, BaseAmount);
			Actor.HandleEvent(getRandomBuyChimericBodyPartRollsEvent);
			return getRandomBuyChimericBodyPartRollsEvent.Amount;
		}
		return BaseAmount;
	}
}
