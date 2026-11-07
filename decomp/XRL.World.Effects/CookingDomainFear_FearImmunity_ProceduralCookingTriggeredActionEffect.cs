using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFear_FearImmunity_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingEffect
{
	public CookingDomainFear_FearImmunity_ProceduralCookingTriggeredActionEffect()
	{
		Duration = 300;
	}

	public override string GetDetails()
	{
		return "Immune to fear.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplyFear");
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
		Object.UnregisterEffectEvent(this, "ApplyFear");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyFear")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
