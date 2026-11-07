using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainPhase_UnitDoublePhaseDuration : ProceduralCookingEffectUnit
{
	public GameObject Object;

	public override string GetDescription()
	{
		return "Phase effects last twice as long.";
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.RegisterEffectEvent(parent, "ApplyEffect");
	}

	public override void Remove(GameObject Object, Effect parent)
	{
		Object.UnregisterEffectEvent(parent, "ApplyEffect");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "ApplyEffect" && E.GetParameter("Effect") is Effect effect && effect.GetType().Name == "Phased")
		{
			effect.Duration *= 2;
		}
	}
}
