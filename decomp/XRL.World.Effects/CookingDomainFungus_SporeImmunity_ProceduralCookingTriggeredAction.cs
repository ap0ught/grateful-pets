using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFungus_SporeImmunity_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they are immune to fungal spores for 6 hours.";
	}

	public override string GetNotification()
	{
		return "@they become impervious to fungal spores.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainFungus_SporeImmunity_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainFungus_SporeImmunity_ProceduralCookingTriggeredActionEffect());
		}
	}
}
