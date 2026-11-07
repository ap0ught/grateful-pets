using System;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class GetSpringinessEvent : PooledEvent<GetSpringinessEvent>
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

	public static GetSpringinessEvent FromPool(GameObject Object, int Base, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		GetSpringinessEvent getSpringinessEvent = PooledEvent<GetSpringinessEvent>.FromPool();
		getSpringinessEvent.Object = Object;
		getSpringinessEvent.Base = Base;
		getSpringinessEvent.PercentageIncrease = PercentageIncrease;
		getSpringinessEvent.LinearIncrease = LinearIncrease;
		getSpringinessEvent.PercentageReduction = PercentageReduction;
		getSpringinessEvent.LinearReduction = LinearReduction;
		return getSpringinessEvent;
	}

	public static int GetFor(GameObject Object, int PercentageIncrease = 0, int LinearIncrease = 0, int PercentageReduction = 0, int LinearReduction = 0)
	{
		int intProperty = Object.GetIntProperty("Springiness");
		int val;
		if (GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetSpringinessEvent>.ID, MinEvent.CascadeLevel))
		{
			GetSpringinessEvent getSpringinessEvent = FromPool(Object, intProperty, PercentageIncrease, LinearIncrease, PercentageReduction, LinearReduction);
			Object.HandleEvent(getSpringinessEvent);
			val = (intProperty + getSpringinessEvent.LinearIncrease) * (100 + getSpringinessEvent.PercentageIncrease) * (100 - getSpringinessEvent.PercentageReduction) / 10000 - getSpringinessEvent.LinearReduction;
		}
		else
		{
			val = (intProperty + LinearIncrease) * (100 + PercentageIncrease) * (100 - PercentageReduction) / 10000 - LinearReduction;
		}
		return Math.Max(val, 0);
	}
}
