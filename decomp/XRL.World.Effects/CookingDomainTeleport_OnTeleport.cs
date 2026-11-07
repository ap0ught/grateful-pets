namespace XRL.World.Effects;

public class CookingDomainTeleport_OnTeleport : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlyEnflamed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature teleport@s,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterTeleport");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterTeleport")
		{
			Trigger();
		}
		return base.FireEvent(E);
	}
}
