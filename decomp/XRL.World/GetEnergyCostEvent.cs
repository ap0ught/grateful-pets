namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Singleton)]
public class GetEnergyCostEvent : SingletonEvent<GetEnergyCostEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public int BaseAmount;

	public int Amount;

	public string Type;

	public int PercentageReduction;

	public int LinearReduction;

	public bool Passive;

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
		BaseAmount = 0;
		Amount = 0;
		Type = null;
		PercentageReduction = 0;
		LinearReduction = 0;
		Passive = false;
	}

	public bool TypeMatches(string lookFor)
	{
		if (string.IsNullOrEmpty(Type))
		{
			return false;
		}
		return Type.Contains(lookFor);
	}

	public static int GetFor(GameObject Actor, int BaseAmount, string Type, int PercentageReduction = 0, int LinearReduction = 0, int MinAmount = 0, bool Passive = false)
	{
		int num = BaseAmount;
		if (Actor.HasRegisteredEvent("UsingEnergy"))
		{
			Event obj = Event.New("UsingEnergy", "Amount", num, "Type", Type);
			Actor.FireEvent(obj);
			int intParameter = obj.GetIntParameter("Amount");
			if (intParameter != num)
			{
				LinearReduction += num - intParameter;
			}
		}
		if (Actor.WantEvent(SingletonEvent<GetEnergyCostEvent>.ID, CascadeLevel))
		{
			SingletonEvent<GetEnergyCostEvent>.Instance.Actor = Actor;
			SingletonEvent<GetEnergyCostEvent>.Instance.BaseAmount = BaseAmount;
			SingletonEvent<GetEnergyCostEvent>.Instance.Amount = num;
			SingletonEvent<GetEnergyCostEvent>.Instance.Type = Type;
			SingletonEvent<GetEnergyCostEvent>.Instance.PercentageReduction = PercentageReduction;
			SingletonEvent<GetEnergyCostEvent>.Instance.LinearReduction = LinearReduction;
			SingletonEvent<GetEnergyCostEvent>.Instance.Passive = Passive;
			Actor.HandleEvent(SingletonEvent<GetEnergyCostEvent>.Instance);
			num = SingletonEvent<GetEnergyCostEvent>.Instance.Amount;
			PercentageReduction = SingletonEvent<GetEnergyCostEvent>.Instance.PercentageReduction;
			LinearReduction = SingletonEvent<GetEnergyCostEvent>.Instance.LinearReduction;
		}
		if (PercentageReduction != 0)
		{
			num = num * (100 - PercentageReduction) / 100;
		}
		if (LinearReduction != 0)
		{
			num -= LinearReduction;
		}
		if (num < MinAmount)
		{
			num = MinAmount;
		}
		return num;
	}
}
