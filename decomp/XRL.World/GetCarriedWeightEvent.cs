using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Pool)]
public class GetCarriedWeightEvent : IWeightEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetCarriedWeightEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 0;

	private static List<GetCarriedWeightEvent> Pool;

	private static int PoolCounter;

	public GetCarriedWeightEvent()
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

	public static void ResetTo(ref GetCarriedWeightEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetCarriedWeightEvent FromPool()
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

	public static int GetFor(GameObject Object)
	{
		double num = 0.0;
		if (Object != null)
		{
			if (Object.HasRegisteredEvent("GetCarriedWeight"))
			{
				Event obj = Event.New("GetCarriedWeight", "Weight", (int)num);
				if (!Object.FireEvent(obj))
				{
					return obj.GetIntParameter("Weight");
				}
				num = obj.GetIntParameter("Weight");
			}
			if (Object.WantEvent(ID, CascadeLevel))
			{
				GetCarriedWeightEvent getCarriedWeightEvent = FromPool();
				getCarriedWeightEvent.Weight = num;
				if (!Object.HandleEvent(getCarriedWeightEvent))
				{
					return (int)getCarriedWeightEvent.Weight;
				}
				num = getCarriedWeightEvent.Weight;
			}
		}
		return (int)num;
	}
}
