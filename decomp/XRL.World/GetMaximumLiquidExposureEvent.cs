using System;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetMaximumLiquidExposureEvent : PooledEvent<GetMaximumLiquidExposureEvent>
{
	public GameObject Object;

	public int Base;

	public int PercentageIncrease;

	public int LinearIncrease;

	public int PercentageReduction;

	public int LinearReduction;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Base = 0;
		PercentageIncrease = 0;
		LinearIncrease = 0;
		PercentageReduction = 0;
		LinearReduction = 0;
	}

	public static GetMaximumLiquidExposureEvent FromPool(GameObject Object, int Base, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		GetMaximumLiquidExposureEvent getMaximumLiquidExposureEvent = PooledEvent<GetMaximumLiquidExposureEvent>.FromPool();
		getMaximumLiquidExposureEvent.Object = Object;
		getMaximumLiquidExposureEvent.Base = Base;
		getMaximumLiquidExposureEvent.PercentageIncrease = PercentageIncrease;
		getMaximumLiquidExposureEvent.LinearIncrease = LinearIncrease;
		getMaximumLiquidExposureEvent.PercentageReduction = PercentageReduction;
		getMaximumLiquidExposureEvent.LinearReduction = LinearReduction;
		return getMaximumLiquidExposureEvent;
	}

	public static int GetFor(GameObject Object, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		int num = GetBase(Object);
		int val;
		if (Object != null && Object.WantEvent(PooledEvent<GetMaximumLiquidExposureEvent>.ID, MinEvent.CascadeLevel))
		{
			GetMaximumLiquidExposureEvent getMaximumLiquidExposureEvent = FromPool(Object, num, PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getMaximumLiquidExposureEvent);
			val = (int)Math.Round((double)(num + getMaximumLiquidExposureEvent.LinearIncrease) * (100.0 + (double)getMaximumLiquidExposureEvent.PercentageIncrease) * (100.0 - (double)getMaximumLiquidExposureEvent.PercentageReduction) / 10000.0) - getMaximumLiquidExposureEvent.LinearReduction;
		}
		else
		{
			val = (int)Math.Round((double)(num + LinearIncrease) * (100.0 + (double)PercentageIncrease) * (100.0 - (double)PercentageReduction) / 10000.0) - LinearReduction;
		}
		return Math.Max(val, 0);
	}

	public static double GetDoubleFor(GameObject Object, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		double doubleBase = GetDoubleBase(Object);
		double val;
		if (Object != null && Object.WantEvent(PooledEvent<GetMaximumLiquidExposureEvent>.ID, MinEvent.CascadeLevel))
		{
			GetMaximumLiquidExposureEvent getMaximumLiquidExposureEvent = FromPool(Object, (int)Math.Round(doubleBase), PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getMaximumLiquidExposureEvent);
			val = (doubleBase + (double)getMaximumLiquidExposureEvent.LinearIncrease) * (double)(100 + getMaximumLiquidExposureEvent.PercentageIncrease) * (double)(100 - getMaximumLiquidExposureEvent.PercentageReduction) / 10000.0 - (double)getMaximumLiquidExposureEvent.LinearReduction;
		}
		else
		{
			val = (doubleBase + (double)LinearIncrease) * (double)(100 + PercentageIncrease) * (double)(100 - PercentageReduction) / 10000.0 - (double)LinearReduction;
		}
		return Math.Max(val, 0.0);
	}

	public static int GetBase(GameObject obj)
	{
		if (obj == null)
		{
			return 0;
		}
		if (obj.HasTag("Creature"))
		{
			return Math.Max(obj.Stat("Strength") + obj.Stat("Toughness") + obj.GetConcreteBodyPartCount(), 1);
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return 0;
		}
		return (int)Math.Round(Math.Max(obj.GetIntrinsicWeight().DiminishingReturns(1.0), 1.0));
	}

	public static double GetDoubleBase(GameObject obj)
	{
		if (obj == null)
		{
			return 0.0;
		}
		if (obj.HasTag("Creature"))
		{
			return Math.Max(obj.Stat("Strength") + obj.Stat("Toughness") + obj.GetConcreteBodyPartCount(), 1);
		}
		if (obj.HasTag("ExcavatoryTerrainFeature"))
		{
			return 0.0;
		}
		return Math.Max(obj.GetIntrinsicWeight().DiminishingReturns(1.0), 1.0);
	}
}
