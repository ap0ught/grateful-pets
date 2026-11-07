namespace XRL.World.Effects;

public class CookingDomainFear_OnFeared : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature become@s afraid,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("FearApplied");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "FearApplied")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
