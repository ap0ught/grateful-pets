namespace XRL.World.Effects;

public class CookingDomainLove_OnGainFollower : ProceduralCookingEffectWithTrigger
{
	public bool CurrentlySlowed;

	public override string GetTriggerDescription()
	{
		return "whenever @thisCreature gain@s a new follower,";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("GainedNewFollower");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "GainedNewFollower")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && !gameObjectParameter.IsTrifling)
			{
				Trigger();
			}
		}
		return base.FireEvent(E);
	}
}
