namespace XRL.World.Effects;

public class CookingDomainAgility_OnPerformCriticalHit : ProceduralCookingEffectWithTrigger
{
	public int Tier;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature perform@s a critical hit,";
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature perform@s a critical hit,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerCriticalHit");
		Registrar.Register("MissileAttackerCriticalHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerCriticalHit" || E.ID == "MissileAttackerCriticalHit")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
