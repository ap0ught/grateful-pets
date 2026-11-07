namespace XRL.World.Effects;

public class OnKillToughProceduralCookingTrigger : ProceduralCookingEffectWithTrigger
{
	public override string GetTriggerDescription()
	{
		return "when you kill a tough mob";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Killed");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Killed")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && gameObjectParameter.Con(null, IgnoreHideCon: true) >= 5)
			{
				Trigger();
			}
			Remove(Object);
		}
		return base.FireEvent(E);
	}
}
