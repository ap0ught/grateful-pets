using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFungus_SporeImmunity_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingEffect
{
	public CookingDomainFungus_SporeImmunity_ProceduralCookingTriggeredActionEffect()
	{
		Duration = 300;
	}

	public override string GetDetails()
	{
		return "Immune to fungal spores.";
	}

	public override bool Apply(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "ApplySpores");
		return base.Apply(Object);
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
		Object.UnregisterEffectEvent(this, "ApplySpores");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplySpores")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
