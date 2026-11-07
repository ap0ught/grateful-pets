namespace XRL.World.Effects;

public class CookingDomainRegenLowtier_OnSalve : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature use@s a salve or ubernostrum injector,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyingSalve");
		Registrar.Register("ApplyingUbernostrum");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyingSalve" || E.ID == "ApplyingUbernostrum")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
