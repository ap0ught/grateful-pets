using System;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetTradePerformanceEvent : PooledEvent<GetTradePerformanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Trader;

	public int BaseRating;

	public double LinearAdjustment;

	public double FactorAdjustment;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Trader = null;
		BaseRating = 0;
		LinearAdjustment = 0.0;
		FactorAdjustment = 0.0;
	}

	public static GetTradePerformanceEvent FromPool(GameObject Actor, GameObject Trader, int BaseRating, double LinearAdjustment = 0.0, double FactorAdjustment = 1.0)
	{
		GetTradePerformanceEvent getTradePerformanceEvent = PooledEvent<GetTradePerformanceEvent>.FromPool();
		getTradePerformanceEvent.Actor = Actor;
		getTradePerformanceEvent.Trader = Trader;
		getTradePerformanceEvent.BaseRating = BaseRating;
		getTradePerformanceEvent.LinearAdjustment = LinearAdjustment;
		getTradePerformanceEvent.FactorAdjustment = FactorAdjustment;
		return getTradePerformanceEvent;
	}

	public static double GetFor(GameObject Actor, GameObject Trader)
	{
		if (Trader == null || Actor == null)
		{
			return 1.0;
		}
		if (!Actor.HasStat("Ego"))
		{
			return 0.25;
		}
		int num = Actor.StatMod("Ego");
		double num2 = 0.0;
		double num3 = 1.0;
		bool flag = true;
		if (flag && Actor.HasRegisteredEvent("GetTradePerformance"))
		{
			Event obj = Event.New("GetTradePerformance");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Trader", Trader);
			obj.SetParameter("BaseRating", num);
			obj.SetParameter("LinearAdjustment", num2);
			obj.SetParameter("FactorAdjustment", num3);
			if (!Actor.FireEvent(obj))
			{
				flag = false;
			}
			num2 = (double)obj.GetParameter("LinearAdjustment");
			num3 = (double)obj.GetParameter("FactorAdjustment");
		}
		if (flag && Actor.WantEvent(PooledEvent<GetTradePerformanceEvent>.ID, CascadeLevel))
		{
			GetTradePerformanceEvent getTradePerformanceEvent = FromPool(Actor, Trader, num, num2, num3);
			if (!Actor.HandleEvent(getTradePerformanceEvent))
			{
				flag = false;
			}
			num2 = getTradePerformanceEvent.LinearAdjustment;
			num3 = getTradePerformanceEvent.FactorAdjustment;
		}
		return Math.Min(Math.Max((0.35 + 0.07 * ((double)num + num2)) * num3, 0.05), 0.95);
	}

	public static int GetRatingFor(GameObject Actor, GameObject Trader)
	{
		if (Trader == null || Actor == null)
		{
			return 0;
		}
		if (!Actor.HasStat("Ego"))
		{
			return -2;
		}
		int num = Actor.StatMod("Ego");
		double num2 = 0.0;
		double num3 = 1.0;
		bool flag = true;
		if (flag && Actor.HasRegisteredEvent("GetTradePerformance"))
		{
			Event obj = Event.New("GetTradePerformance");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Trader", Trader);
			obj.SetParameter("BaseRating", num);
			obj.SetParameter("LinearAdjustment", num2);
			obj.SetParameter("FactorAdjustment", num3);
			if (!Actor.FireEvent(obj))
			{
				flag = false;
			}
			num2 = (double)obj.GetParameter("LinearAdjustment");
			num3 = (double)obj.GetParameter("FactorAdjustment");
		}
		if (flag && Actor.WantEvent(PooledEvent<GetTradePerformanceEvent>.ID, CascadeLevel))
		{
			GetTradePerformanceEvent getTradePerformanceEvent = FromPool(Actor, Trader, num, num2, num3);
			if (!Actor.HandleEvent(getTradePerformanceEvent))
			{
				flag = false;
			}
			num2 = getTradePerformanceEvent.LinearAdjustment;
			num3 = getTradePerformanceEvent.FactorAdjustment;
		}
		return (int)(((double)num + num2) * num3);
	}
}
