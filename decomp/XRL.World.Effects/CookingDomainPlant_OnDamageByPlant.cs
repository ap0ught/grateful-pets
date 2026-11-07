namespace XRL.World.Effects;

public class CookingDomainPlant_OnDamageByPlant : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature take@s damage from a plant,";
	}

	public override string GetTemplatedTriggerDescription()
	{
		return "whenever @thisCreature take@s damage from a plant,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("TookDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "TookDamage")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			Damage parameter = E.GetParameter<Damage>("Damage");
			if ((gameObjectParameter != null && (gameObjectParameter.GetBlueprint().DescendsFrom("Plant") || gameObjectParameter.GetBlueprint().DescendsFrom("MutatedPlant") || gameObjectParameter.HasTagOrProperty("Plant"))) || (parameter != null && parameter.HasAttribute("Plant")))
			{
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
