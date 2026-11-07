using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainFear_FearImmunity_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they become@s immune to fear for 6 hours.";
	}

	public override string GetNotification()
	{
		return "@they become@s immune to fear.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainFear_FearImmunity_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainFear_FearImmunity_ProceduralCookingTriggeredActionEffect());
		}
	}
}
