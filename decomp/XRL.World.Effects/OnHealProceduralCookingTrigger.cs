namespace XRL.World.Effects;

public class OnHealProceduralCookingTrigger : ProceduralCookingEffectWithTrigger
{
	public bool Armed;

	public override void Init(GameObject target)
	{
		base.Init(target);
		Armed = false;
	}

	public override string GetTriggerDescription()
	{
		return "when you heal to full health from half";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (!Object.isDamaged() && Armed)
			{
				Armed = false;
				Trigger();
			}
			else if (Object.isDamaged(0.5, inclusive: true))
			{
				Armed = true;
			}
		}
		return base.FireEvent(E);
	}
}
