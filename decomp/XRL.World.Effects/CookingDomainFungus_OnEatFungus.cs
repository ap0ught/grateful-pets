namespace XRL.World.Effects;

public class CookingDomainFungus_OnEatFungus : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature eat@s a mushroom,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Eating");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Eating")
		{
			if (!(E.GetParameter("Food") is GameObject gameObject))
			{
				return true;
			}
			if (gameObject.GetBlueprint().InheritsFrom("Mushroom") || gameObject.HasTag("Mushroom"))
			{
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
