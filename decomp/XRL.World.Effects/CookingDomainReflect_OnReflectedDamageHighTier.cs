namespace XRL.World.Effects;

public class CookingDomainReflect_OnReflectedDamageHighTier : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature reflect@s damage, there's a 100% chance";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ReflectedDamage");
		base.Register(Object, Registrar);
	}

	public override void Init(GameObject target)
	{
		base.Init(target);
		CurrentlyEnflamed = false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ReflectedDamage")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
