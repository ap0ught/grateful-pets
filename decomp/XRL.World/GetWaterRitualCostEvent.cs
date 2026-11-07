namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class GetWaterRitualCostEvent : PooledEvent<GetWaterRitualCostEvent>
{
	public new static readonly int CascadeLevel = 3;

	public GameObject Actor;

	public GameObject Target;

	public string Type;

	public int BaseCost;

	public int Cost;

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
		Target = null;
		Type = null;
		BaseCost = 0;
		Cost = 0;
	}

	public static GetWaterRitualCostEvent FromPool(GameObject Actor, GameObject Target, string Type, int BaseCost, int Cost)
	{
		GetWaterRitualCostEvent getWaterRitualCostEvent = PooledEvent<GetWaterRitualCostEvent>.FromPool();
		getWaterRitualCostEvent.Actor = Actor;
		getWaterRitualCostEvent.Target = Target;
		getWaterRitualCostEvent.Type = Type;
		getWaterRitualCostEvent.BaseCost = BaseCost;
		getWaterRitualCostEvent.Cost = Cost;
		return getWaterRitualCostEvent;
	}

	public static int GetFor(GameObject Actor, GameObject Target, string Type, int BaseCost)
	{
		int num = BaseCost;
		if (Actor.HasRegisteredEvent("GetWaterRitualCost"))
		{
			Event obj = Event.New("GetWaterRitualCost", 2, 1, 2);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Target", Target);
			obj.SetParameter("Type", Type);
			obj.SetParameter("BaseCost", BaseCost);
			obj.SetParameter("Cost", num);
			if (!Actor.FireEvent(obj))
			{
				return obj.GetIntParameter("Cost");
			}
			num = obj.GetIntParameter("Cost");
		}
		if (Actor.WantEvent(PooledEvent<GetWaterRitualCostEvent>.ID, CascadeLevel))
		{
			GetWaterRitualCostEvent getWaterRitualCostEvent = FromPool(Actor, Target, Type, BaseCost, num);
			if (!Actor.HandleEvent(getWaterRitualCostEvent))
			{
				return getWaterRitualCostEvent.Cost;
			}
			num = getWaterRitualCostEvent.Cost;
		}
		return num;
	}
}
