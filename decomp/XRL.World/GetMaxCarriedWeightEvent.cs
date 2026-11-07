using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class GetMaxCarriedWeightEvent : IWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetMaxCarriedWeightEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 1;

	private static List<GetMaxCarriedWeightEvent> Pool;

	private static int PoolCounter;

	public GetMaxCarriedWeightEvent()
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

	public static void ResetTo(ref GetMaxCarriedWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetMaxCarriedWeightEvent FromPool()
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

	public void AdjustWeight(double Factor)
	{
		Weight *= Factor;
	}

	public static int GetFor(GameObject Object, double BaseWeight)
	{
		double num = BaseWeight;
		if (Object != null)
		{
			bool flag = true;
			if (Object.IsGiganticCreature)
			{
				num *= 2.0;
			}
			if (Object.HasRegisteredEvent("GetMaxWeight"))
			{
				Event obj = Event.New("GetMaxWeight", "BaseWeight", (int)BaseWeight, "Weight", (int)num);
				flag = Object.FireEvent(obj);
				BaseWeight = obj.GetIntParameter("BaseWeight");
				num = obj.GetIntParameter("Weight");
			}
			if (flag && Object.WantEvent(ID, CascadeLevel))
			{
				GetMaxCarriedWeightEvent getMaxCarriedWeightEvent = FromPool();
				getMaxCarriedWeightEvent.Object = Object;
				getMaxCarriedWeightEvent.BaseWeight = BaseWeight;
				getMaxCarriedWeightEvent.Weight = num;
				Object.HandleEvent(getMaxCarriedWeightEvent);
				num = getMaxCarriedWeightEvent.Weight;
			}
		}
		return (int)num;
	}
}
