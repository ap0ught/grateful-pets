namespace XRL.World.Effects;

public class CookingDomainFungus_OnItchy : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature get@s itchy skin,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeApplyFungalInfection");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyFungalInfection")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
