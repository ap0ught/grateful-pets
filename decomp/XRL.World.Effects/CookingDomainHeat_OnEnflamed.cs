namespace XRL.World.Effects;

public class CookingDomainHeat_OnEnflamed : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature @is set on fire,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
		CurrentlyEnflamed = false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (CurrentlyEnflamed)
			{
				if (Object.Physics.Temperature < Object.Physics.FlameTemperature)
				{
					CurrentlyEnflamed = false;
				}
			}
			else if (Object.Physics.Temperature >= Object.Physics.FlameTemperature)
			{
				CurrentlyEnflamed = true;
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
