using System;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetSprintDurationEvent : PooledEvent<GetSprintDurationEvent>
{
	public GameObject Object;

	public int Base;

	public int PercentageIncrease;

	public int LinearIncrease;

	public int PercentageReduction;

	public int LinearReduction;

	public Templates.StatCollector Stats;

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
		Stats = null;
	}

	public static GetSprintDurationEvent FromPool(GameObject Object, int Base, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0, Templates.StatCollector Stats = null)
	{
		GetSprintDurationEvent getSprintDurationEvent = PooledEvent<GetSprintDurationEvent>.FromPool();
		getSprintDurationEvent.Object = Object;
		getSprintDurationEvent.Base = Base;
		getSprintDurationEvent.PercentageIncrease = PercentageIncrease;
		getSprintDurationEvent.LinearIncrease = LinearIncrease;
		getSprintDurationEvent.PercentageReduction = PercentageReduction;
		getSprintDurationEvent.LinearReduction = LinearReduction;
		getSprintDurationEvent.Stats = Stats;
		return getSprintDurationEvent;
	}

	public static int GetFor(GameObject Object, int Base = 0, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0, Templates.StatCollector Stats = null)
	{
		int val;
		if (Object != null && Object.WantEvent(PooledEvent<GetSprintDurationEvent>.ID, MinEvent.CascadeLevel))
		{
			GetSprintDurationEvent getSprintDurationEvent = FromPool(Object, Base, PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction, Stats);
			Object.HandleEvent(getSprintDurationEvent);
			val = (Base + getSprintDurationEvent.LinearIncrease) * (100 + getSprintDurationEvent.PercentageIncrease) * (100 - getSprintDurationEvent.PercentageReduction) / 10000 - getSprintDurationEvent.LinearReduction;
			Stats?.CollectBonusModifiers("Duration", Base);
		}
		else
		{
			val = (Base + LinearIncrease) * (100 + PercentageIncrease) * (100 - PercentageReduction) / 10000 - LinearReduction;
			Stats?.CollectBonusModifiers("Duration", Base);
		}
		return Math.Max(val, 0);
	}
}
