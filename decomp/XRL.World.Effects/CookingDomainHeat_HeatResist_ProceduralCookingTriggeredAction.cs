using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHeat_HeatResist_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain 40-50 Heat Resist for 6 hours.";
	}

	public override string GetNotification()
	{
		return "@they become fortified against the heat.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainHeat_HeatResist_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainHeat_HeatResist_ProceduralCookingTriggeredActionEffect());
		}
	}
}
