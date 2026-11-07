using System;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetKineticResistanceEvent : PooledEvent<GetKineticResistanceEvent>
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

	public static GetKineticResistanceEvent FromPool(GameObject Object, int Base, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		GetKineticResistanceEvent getKineticResistanceEvent = PooledEvent<GetKineticResistanceEvent>.FromPool();
		getKineticResistanceEvent.Object = Object;
		getKineticResistanceEvent.Base = Base;
		getKineticResistanceEvent.PercentageIncrease = PercentageIncrease;
		getKineticResistanceEvent.LinearIncrease = LinearIncrease;
		getKineticResistanceEvent.PercentageReduction = PercentageReduction;
		getKineticResistanceEvent.LinearReduction = LinearReduction;
		return getKineticResistanceEvent;
	}

	public static int GetFor(GameObject Object, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		int num = Object.Weight + Object.GetIntProperty("Anchoring");
		int val;
		if (Object != null && Object.WantEvent(PooledEvent<GetKineticResistanceEvent>.ID, MinEvent.CascadeLevel))
		{
			GetKineticResistanceEvent getKineticResistanceEvent = FromPool(Object, num, PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getKineticResistanceEvent);
			val = (num + getKineticResistanceEvent.LinearIncrease) * (100 + getKineticResistanceEvent.PercentageIncrease) * (100 - getKineticResistanceEvent.PercentageReduction) / 10000 - getKineticResistanceEvent.LinearReduction;
		}
		else
		{
			val = (num + LinearIncrease) * (100 + PercentageIncrease) * (100 - PercentageReduction) / 10000 - LinearReduction;
		}
		return Math.Max(val, 0);
	}
}
