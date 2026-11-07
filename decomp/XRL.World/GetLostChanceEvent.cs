using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetLostChanceEvent : ITravelEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetLostChanceEvent), null, CountPool, ResetPool);

	private static List<GetLostChanceEvent> Pool;

	private static int PoolCounter;

	public int DefaultLimit;

	public bool OverrideDefaultLimit;

	public GetLostChanceEvent()
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

	public static void ResetTo(ref GetLostChanceEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetLostChanceEvent FromPool()
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
		DefaultLimit = 0;
		OverrideDefaultLimit = false;
	}

	public static int GetFor(GameObject Actor, string TravelClass = null, int PercentageBonus = 0, int DefaultLimit = 95, bool OverrideDefaultLimit = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetLostChance"))
		{
			Event obj = Event.New("GetLostChance");
			obj.SetParameter("Object", Actor);
			obj.SetParameter("TravelClass", TravelClass);
			obj.SetParameter("PercentageBonus", PercentageBonus);
			obj.SetFlag("OverrideDefaultLimit", OverrideDefaultLimit);
			flag = Actor.FireEvent(obj);
			PercentageBonus = obj.GetIntParameter("PercentageBonus");
			OverrideDefaultLimit = obj.HasFlag("OverrideDefaultLimit");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(ID, ITravelEvent.CascadeLevel))
		{
			GetLostChanceEvent getLostChanceEvent = FromPool();
			getLostChanceEvent.Actor = Actor;
			getLostChanceEvent.TravelClass = TravelClass;
			getLostChanceEvent.PercentageBonus = PercentageBonus;
			flag = Actor.HandleEvent(getLostChanceEvent);
			PercentageBonus = getLostChanceEvent.PercentageBonus;
			OverrideDefaultLimit = getLostChanceEvent.OverrideDefaultLimit;
		}
		if (!OverrideDefaultLimit && PercentageBonus > DefaultLimit)
		{
			PercentageBonus = DefaultLimit;
		}
		return PercentageBonus;
	}
}
