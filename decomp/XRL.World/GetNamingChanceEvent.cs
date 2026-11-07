using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class GetNamingChanceEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetNamingChanceEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 1;

	private static List<GetNamingChanceEvent> Pool;

	private static int PoolCounter;

	public double Base;

	public double PercentageBonus;

	public double LinearBonus;

	public GetNamingChanceEvent()
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

	public static void ResetTo(ref GetNamingChanceEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetNamingChanceEvent FromPool()
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
		Base = 0.0;
		PercentageBonus = 0.0;
		LinearBonus = 0.0;
	}

	public static GetNamingChanceEvent FromPool(GameObject Actor, GameObject Item, double Base, double PercentageBonus = 0.0, double LinearBonus = 0.0)
	{
		GetNamingChanceEvent getNamingChanceEvent = FromPool();
		getNamingChanceEvent.Actor = Actor;
		getNamingChanceEvent.Item = Item;
		getNamingChanceEvent.Base = Base;
		getNamingChanceEvent.PercentageBonus = PercentageBonus;
		getNamingChanceEvent.LinearBonus = LinearBonus;
		return getNamingChanceEvent;
	}

	public static double GetFor(GameObject Actor, GameObject Item, double Base, double PercentageBonus = 0.0, double LinearBonus = 0.0)
	{
		bool flag = Actor?.WantEvent(ID, CascadeLevel) ?? false;
		bool flag2 = Item?.WantEvent(ID, CascadeLevel) ?? false;
		if (flag | flag2)
		{
			GetNamingChanceEvent getNamingChanceEvent = FromPool(Actor, Item, Base, PercentageBonus, LinearBonus);
			bool flag3 = true;
			if (flag)
			{
				flag3 = Actor.HandleEvent(getNamingChanceEvent);
			}
			if (flag3 & flag2)
			{
				flag3 = Item.HandleEvent(getNamingChanceEvent);
			}
			return Base * (100.0 + getNamingChanceEvent.PercentageBonus) / 100.0 + getNamingChanceEvent.LinearBonus;
		}
		return Base * (100.0 + PercentageBonus) / 100.0 + LinearBonus;
	}
}
