namespace XRL.World.Effects;

public class CookingDomainRubber_OnProne : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature go@es prone,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EffectApplied");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EffectApplied" && E.GetParameter("Effect") is Prone)
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
