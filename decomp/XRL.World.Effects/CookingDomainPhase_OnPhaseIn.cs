namespace XRL.World.Effects;

public class CookingDomainPhase_OnPhaseIn : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature phase@s in,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterPhaseIn");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterPhaseIn")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
