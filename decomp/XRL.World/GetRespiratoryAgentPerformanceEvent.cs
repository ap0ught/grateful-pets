using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetRespiratoryAgentPerformanceEvent : PooledEvent<GetRespiratoryAgentPerformanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public GameObject GasObject;

	public Gas Gas;

	public int BaseRating;

	public int LinearAdjustment;

	public int PercentageAdjustment;

	public bool WillAllowSave;

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
		Object = null;
		GasObject = null;
		Gas = null;
		BaseRating = 0;
		LinearAdjustment = 0;
		PercentageAdjustment = 0;
		WillAllowSave = false;
	}

	public static int GetFor(GameObject Object, GameObject GasObject = null, Gas Gas = null, int? BaseRating = null, int LinearAdjustment = 0, int PercentageAdjustment = 0, bool WillAllowSave = false)
	{
		if (!GameObject.Validate(ref Object))
		{
			return 1;
		}
		if (Gas == null && GasObject != null)
		{
			Gas = GasObject.GetPart<Gas>();
		}
		int num = BaseRating ?? Gas?.Density ?? 0;
		bool flag = true;
		if (flag)
		{
			bool flag2 = Object.HasRegisteredEvent("GetRespiratoryAgentPerformance");
			bool flag3 = GasObject?.HasRegisteredEvent("GetRespiratoryAgentPerformance") ?? false;
			if (flag2 | flag3)
			{
				Event obj = Event.New("GetRespiratoryAgentPerformance");
				obj.SetParameter("Object", Object);
				obj.SetParameter("GasObject", GasObject);
				obj.SetParameter("Gas", Gas);
				obj.SetParameter("BaseRating", num);
				obj.SetParameter("LinearAdjustment", LinearAdjustment);
				obj.SetParameter("PercentageAdjustment", PercentageAdjustment);
				obj.SetFlag("WillAllowSave", WillAllowSave);
				if ((flag & flag2) && !Object.FireEvent(obj))
				{
					flag = false;
				}
				if ((flag & flag3) && !GasObject.FireEvent(obj))
				{
					flag = false;
				}
				LinearAdjustment = obj.GetIntParameter("LinearAdjustment");
				PercentageAdjustment = obj.GetIntParameter("PercentageAdjustment");
			}
		}
		if (flag)
		{
			bool flag4 = Object.WantEvent(PooledEvent<GetRespiratoryAgentPerformanceEvent>.ID, CascadeLevel);
			bool flag5 = GasObject?.WantEvent(PooledEvent<GetRespiratoryAgentPerformanceEvent>.ID, CascadeLevel) ?? false;
			if (flag4 | flag5)
			{
				GetRespiratoryAgentPerformanceEvent getRespiratoryAgentPerformanceEvent = PooledEvent<GetRespiratoryAgentPerformanceEvent>.FromPool();
				getRespiratoryAgentPerformanceEvent.Object = Object;
				getRespiratoryAgentPerformanceEvent.GasObject = GasObject;
				getRespiratoryAgentPerformanceEvent.Gas = Gas;
				getRespiratoryAgentPerformanceEvent.BaseRating = num;
				getRespiratoryAgentPerformanceEvent.LinearAdjustment = LinearAdjustment;
				getRespiratoryAgentPerformanceEvent.PercentageAdjustment = PercentageAdjustment;
				getRespiratoryAgentPerformanceEvent.WillAllowSave = WillAllowSave;
				if ((flag & flag4) && !Object.HandleEvent(getRespiratoryAgentPerformanceEvent))
				{
					flag = false;
				}
				if ((flag & flag5) && !GasObject.HandleEvent(getRespiratoryAgentPerformanceEvent))
				{
					flag = false;
				}
				LinearAdjustment = getRespiratoryAgentPerformanceEvent.LinearAdjustment;
				PercentageAdjustment = getRespiratoryAgentPerformanceEvent.PercentageAdjustment;
			}
		}
		int num2 = num;
		if (LinearAdjustment != 0)
		{
			num2 += LinearAdjustment;
		}
		if (PercentageAdjustment != 0)
		{
			num2 = num2 * (100 + PercentageAdjustment) / 100;
		}
		return num2;
	}
}
