namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetRandomBuyMutationCountEvent : PooledEvent<GetRandomBuyMutationCountEvent>
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

	public static GetRandomBuyMutationCountEvent FromPool(GameObject Actor, int BaseAmount, int Amount)
	{
		GetRandomBuyMutationCountEvent getRandomBuyMutationCountEvent = PooledEvent<GetRandomBuyMutationCountEvent>.FromPool();
		getRandomBuyMutationCountEvent.Actor = Actor;
		getRandomBuyMutationCountEvent.BaseAmount = BaseAmount;
		getRandomBuyMutationCountEvent.Amount = Amount;
		return getRandomBuyMutationCountEvent;
	}

	public static int GetFor(GameObject Actor, int BaseAmount)
	{
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetRandomBuyMutationCountEvent>.ID, MinEvent.CascadeLevel))
		{
			GetRandomBuyMutationCountEvent getRandomBuyMutationCountEvent = FromPool(Actor, BaseAmount, BaseAmount);
			Actor.HandleEvent(getRandomBuyMutationCountEvent);
			return getRandomBuyMutationCountEvent.Amount;
		}
		return BaseAmount;
	}
}
