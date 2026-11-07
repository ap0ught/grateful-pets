using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AwardXPEvent : IXPEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AwardXPEvent), null, CountPool, ResetPool);

	private static List<AwardXPEvent> Pool;

	private static int PoolCounter;

	public AwardXPEvent()
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

	public static void ResetTo(ref AwardXPEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AwardXPEvent FromPool()
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

	public static int Send(GameObject Actor, int Amount, int Tier = -1, int Minimum = 0, int Maximum = int.MaxValue, GameObject Kill = null, GameObject InfluencedBy = null, GameObject PassedUpFrom = null, GameObject PassedDownFrom = null, string ZoneID = null, string Deed = null)
	{
		int amountBefore = Actor.Stat("XP");
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(ID, MinEvent.CascadeLevel))
		{
			AwardXPEvent awardXPEvent = FromPool();
			awardXPEvent.Actor = Actor;
			awardXPEvent.Amount = Amount;
			awardXPEvent.AmountBefore = amountBefore;
			awardXPEvent.Tier = Tier;
			awardXPEvent.Minimum = Minimum;
			awardXPEvent.Maximum = Maximum;
			awardXPEvent.Kill = Kill;
			awardXPEvent.InfluencedBy = InfluencedBy;
			awardXPEvent.PassedUpFrom = PassedUpFrom;
			awardXPEvent.PassedDownFrom = PassedDownFrom;
			awardXPEvent.ZoneID = ZoneID;
			awardXPEvent.Deed = Deed;
			awardXPEvent.TierScaling = true;
			Actor.HandleEvent(awardXPEvent);
			Amount = awardXPEvent.Amount;
		}
		return Amount;
	}
}
