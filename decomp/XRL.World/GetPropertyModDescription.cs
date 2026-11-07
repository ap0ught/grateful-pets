namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class GetPropertyModDescription : PooledEvent<GetPropertyModDescription>
{
	public new static readonly int CascadeLevel = 3;

	public GameObject Actor;

	public string Property;

	public string StatName;

	public Templates.StatCollector Stats;

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
		Property = null;
		StatName = null;
		Stats = null;
	}

	public static GetPropertyModDescription FromPool(GameObject Actor, string Property, string StatName, Templates.StatCollector Stats)
	{
		GetPropertyModDescription getPropertyModDescription = PooledEvent<GetPropertyModDescription>.FromPool();
		getPropertyModDescription.Actor = Actor;
		getPropertyModDescription.Property = Property;
		getPropertyModDescription.StatName = StatName;
		getPropertyModDescription.Stats = Stats;
		return getPropertyModDescription;
	}

	public static bool GetFor(GameObject Actor, string Property, string StatName, Templates.StatCollector Stats)
	{
		if (Actor.WantEvent(PooledEvent<GetPropertyModDescription>.ID, CascadeLevel))
		{
			GetPropertyModDescription e = FromPool(Actor, Property, StatName, Stats);
			if (!Actor.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}

	public void AddLinearBonusModifier(int bonus, string source)
	{
		Stats?.AddLinearBonusModifier(StatName, bonus, source);
	}
}
