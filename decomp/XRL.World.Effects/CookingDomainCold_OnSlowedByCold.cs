namespace XRL.World.Effects;

public class CookingDomainCold_OnSlowedByCold : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlySlowed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature @is slowed by cold,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
		CurrentlySlowed = false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (CurrentlySlowed)
			{
				if (Object.Physics.Temperature > Object.Physics.FreezeTemperature)
				{
					CurrentlySlowed = false;
				}
			}
			else if (Object.Physics.Temperature <= Object.Physics.FreezeTemperature)
			{
				CurrentlySlowed = true;
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
