namespace XRL.World.Effects;

public class CookingDomainTongue_OnStuck : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature get@s stuck, there's a 50% chance";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EffectApplied");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EffectApplied" && E.GetParameter("Effect") is Stuck && 50.in100())
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
