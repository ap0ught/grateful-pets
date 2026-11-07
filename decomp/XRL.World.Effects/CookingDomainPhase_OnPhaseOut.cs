namespace XRL.World.Effects;

public class CookingDomainPhase_OnPhaseOut : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature phase@s out,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterPhaseOut");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterPhaseOut")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
