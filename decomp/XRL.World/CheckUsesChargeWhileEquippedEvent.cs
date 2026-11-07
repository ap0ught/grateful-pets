namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CheckUsesChargeWhileEquippedEvent : PooledEvent<CheckUsesChargeWhileEquippedEvent>
{
	public GameObject Object;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
	}

	public static CheckUsesChargeWhileEquippedEvent FromPool(GameObject Object)
	{
		CheckUsesChargeWhileEquippedEvent checkUsesChargeWhileEquippedEvent = PooledEvent<CheckUsesChargeWhileEquippedEvent>.FromPool();
		checkUsesChargeWhileEquippedEvent.Object = Object;
		return checkUsesChargeWhileEquippedEvent;
	}

	public static bool Check(GameObject Object)
	{
		if (!Object.WantEvent(PooledEvent<CheckUsesChargeWhileEquippedEvent>.ID, MinEvent.CascadeLevel))
		{
			return false;
		}
		if (Object.HandleEvent(FromPool(Object)))
		{
			return false;
		}
		return true;
	}
}
