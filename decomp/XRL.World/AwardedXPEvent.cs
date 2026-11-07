using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AwardedXPEvent : IXPEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AwardedXPEvent), null, CountPool, ResetPool);

	private static List<AwardedXPEvent> Pool;

	private static int PoolCounter;

	public AwardedXPEvent()
	{
		base.ID = ID;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref AwardedXPEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AwardedXPEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public static void Send(IXPEvent ParentEvent, int Amount)
	{
		if (ParentEvent.Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AwardedXPEvent awardedXPEvent = FromPool();
			ParentEvent.ApplyTo(awardedXPEvent);
			awardedXPEvent.Amount = Amount;
			awardedXPEvent.Actor.HandleEvent(awardedXPEvent);
		}
	}
}
