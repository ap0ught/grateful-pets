namespace XRL.World.Effects;

public class CookingDomainFear_OnEatLah : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature eat@s a dreadroot tuber or lah petal,";
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
			if (gameObject.HasTag("Lah"))
			{
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
