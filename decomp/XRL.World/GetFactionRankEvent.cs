using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool, Cascade = 17)]
public class GetFactionRankEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetFactionRankEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 17;

	private static List<GetFactionRankEvent> Pool;

	private static int PoolCounter;

	public GameObject Object;

	public string Faction;

	public string Rank;

	public GetFactionRankEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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

	public static void ResetTo(ref GetFactionRankEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetFactionRankEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Faction = null;
		Rank = null;
	}

	public static string GetFor(GameObject Object, string Faction, string Rank = null)
	{
		if (GameObject.Validate(ref Object) && Object.WantEvent(ID, CascadeLevel))
		{
			GetFactionRankEvent getFactionRankEvent = FromPool();
			getFactionRankEvent.Object = Object;
			getFactionRankEvent.Faction = Faction;
			getFactionRankEvent.Rank = Rank;
			Object.HandleEvent(getFactionRankEvent);
			Rank = getFactionRankEvent.Rank;
		}
		return Rank;
	}
}
