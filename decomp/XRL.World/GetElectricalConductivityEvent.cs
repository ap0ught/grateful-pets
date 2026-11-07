namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetElectricalConductivityEvent : PooledEvent<GetElectricalConductivityEvent>
{
	public new static readonly int CascadeLevel = 17;

	public const int PASSES = 3;

	public GameObject Object;

	public GameObject Source;

	public int Base;

	public int Value;

	public int Phase;

	public int Pass;

	public GameObject ReductionObject;

	public string ReductionReason;

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
		Source = null;
		Base = 0;
		Value = 0;
		Phase = 0;
		Pass = 0;
		ReductionObject = null;
		ReductionReason = null;
	}

	public static int GetFor(GameObject Object, out GameObject ReductionObject, out string ReductionReason, int Base = int.MinValue, GameObject Source = null, int Phase = 0)
	{
		ReductionObject = null;
		ReductionReason = null;
		if (Phase == 0)
		{
			Phase = (GameObject.Validate(ref Source) ? Source.GetPhase() : 5);
		}
		if (Base == int.MinValue && GameObject.Validate(ref Object))
		{
			Base = (Object.PhaseMatches(Phase) ? Object.BaseElectricalConductivity : 0);
		}
		int num = Base;
		bool flag = true;
		Event obj = null;
		GetElectricalConductivityEvent getElectricalConductivityEvent = null;
		int num2 = 1;
		while (flag && num2 <= 3)
		{
			bool flag2 = false;
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetElectricalConductivity"))
			{
				flag2 = true;
				if (obj == null)
				{
					obj = Event.New("GetElectricalConductivity");
					obj.SetParameter("Object", Object);
					obj.SetParameter("Base", Base);
					obj.SetParameter("Source", Source);
					obj.SetParameter("Phase", Phase);
				}
				obj.SetParameter("Value", num);
				obj.SetParameter("Pass", num2);
				obj.SetParameter("ReductionObject", ReductionObject);
				obj.SetParameter("ReductionReason", ReductionReason);
				flag = Object.FireEvent(obj);
				num = obj.GetIntParameter("Value");
				ReductionObject = obj.GetGameObjectParameter("ReductionObject");
				ReductionReason = obj.GetStringParameter("ReductionReason");
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetElectricalConductivityEvent>.ID, CascadeLevel))
			{
				flag2 = true;
				if (getElectricalConductivityEvent == null)
				{
					getElectricalConductivityEvent = PooledEvent<GetElectricalConductivityEvent>.FromPool();
					getElectricalConductivityEvent.Object = Object;
					getElectricalConductivityEvent.Base = Base;
					getElectricalConductivityEvent.Source = Source;
					getElectricalConductivityEvent.Phase = Phase;
				}
				getElectricalConductivityEvent.Value = num;
				getElectricalConductivityEvent.Pass = num2;
				getElectricalConductivityEvent.ReductionObject = ReductionObject;
				getElectricalConductivityEvent.ReductionReason = ReductionReason;
				flag = Object.HandleEvent(getElectricalConductivityEvent);
				num = getElectricalConductivityEvent.Value;
				ReductionObject = getElectricalConductivityEvent.ReductionObject;
				ReductionReason = getElectricalConductivityEvent.ReductionReason;
			}
			if (flag && !flag2)
			{
				flag = false;
			}
			num2++;
		}
		return num;
	}

	public static int GetFor(GameObject Object, int Base = int.MinValue, GameObject Source = null, int Phase = 0)
	{
		GameObject ReductionObject;
		string ReductionReason;
		return GetFor(Object, out ReductionObject, out ReductionReason, Base, Source, Phase);
	}

	public void MinValue(int Amount)
	{
		if (Value < Amount)
		{
			Value = Amount;
		}
	}
}
