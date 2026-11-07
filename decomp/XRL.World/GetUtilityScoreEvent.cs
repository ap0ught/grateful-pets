using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetUtilityScoreEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetUtilityScoreEvent), null, CountPool, ResetPool);

	private static List<GetUtilityScoreEvent> Pool;

	private static int PoolCounter;

	public Damage Damage;

	public bool ForPermission;

	public int Score;

	public GetUtilityScoreEvent()
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

	public static void ResetTo(ref GetUtilityScoreEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetUtilityScoreEvent FromPool()
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

	public override void Reset()
	{
		base.Reset();
		Damage = null;
		ForPermission = false;
		Score = 0;
	}

	public void ApplyScore(int Score)
	{
		if (this.Score < Score)
		{
			this.Score = Score;
		}
	}

	public static int GetFor(GameObject Actor, GameObject Item, Damage Damage = null, bool ForPermission = false)
	{
		if (Item.WantEvent(ID, MinEvent.CascadeLevel))
		{
			GetUtilityScoreEvent getUtilityScoreEvent = FromPool();
			getUtilityScoreEvent.Actor = Actor;
			getUtilityScoreEvent.Item = Item;
			getUtilityScoreEvent.Damage = Damage;
			getUtilityScoreEvent.ForPermission = ForPermission;
			getUtilityScoreEvent.Score = 0;
			Item.HandleEvent(getUtilityScoreEvent);
			return getUtilityScoreEvent.Score;
		}
		return 0;
	}
}
