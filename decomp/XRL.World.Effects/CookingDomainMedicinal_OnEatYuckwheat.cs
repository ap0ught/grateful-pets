namespace XRL.World.Effects;

public class CookingDomainMedicinal_OnEatYuckwheat : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature eat@s an unfermented yuckwheat stem,";
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
			if (gameObject.GetBlueprint().Name == "Yuckwheat Stem" || gameObject.GetBlueprint().InheritsFrom("Yuckwheat Stem") || gameObject.HasTag("Yuckwheat Stem"))
			{
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
