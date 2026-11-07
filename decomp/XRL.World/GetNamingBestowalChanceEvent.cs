using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class GetNamingBestowalChanceEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetNamingBestowalChanceEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 1;

	private static List<GetNamingBestowalChanceEvent> Pool;

	private static int PoolCounter;

	public int Base;

	public int PercentageBonus;

	public int LinearBonus;

	public GetNamingBestowalChanceEvent()
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

	public static void ResetTo(ref GetNamingBestowalChanceEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetNamingBestowalChanceEvent FromPool()
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
		Base = 0;
		PercentageBonus = 0;
		LinearBonus = 0;
	}

	public static GetNamingBestowalChanceEvent FromPool(GameObject Actor, GameObject Item, int Base, int PercentageBonus = 0, int LinearBonus = 0)
	{
		GetNamingBestowalChanceEvent getNamingBestowalChanceEvent = FromPool();
		getNamingBestowalChanceEvent.Actor = Actor;
		getNamingBestowalChanceEvent.Item = Item;
		getNamingBestowalChanceEvent.Base = Base;
		getNamingBestowalChanceEvent.PercentageBonus = PercentageBonus;
		getNamingBestowalChanceEvent.LinearBonus = LinearBonus;
		return getNamingBestowalChanceEvent;
	}

	public static int GetFor(GameObject Actor, GameObject Item, int Base, int PercentageBonus = 0, int LinearBonus = 0)
	{
		bool flag = Actor?.WantEvent(ID, CascadeLevel) ?? false;
		bool flag2 = Item?.WantEvent(ID, CascadeLevel) ?? false;
		if (flag | flag2)
		{
			GetNamingBestowalChanceEvent getNamingBestowalChanceEvent = FromPool(Actor, Item, Base, PercentageBonus, LinearBonus);
			bool flag3 = true;
			if (flag)
			{
				flag3 = Actor.HandleEvent(getNamingBestowalChanceEvent);
			}
			if (flag3 & flag2)
			{
				flag3 = Item.HandleEvent(getNamingBestowalChanceEvent);
			}
			return Base * (100 + getNamingBestowalChanceEvent.PercentageBonus) / 100 + getNamingBestowalChanceEvent.LinearBonus;
		}
		return Base * (100 + PercentageBonus) / 100 + LinearBonus;
	}
}
