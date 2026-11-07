using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHeat_LargeHeatResist_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain 125-175 Heat Resist for 50 turns.";
	}

	public override string GetNotification()
	{
		return "@they become impervious to the heat.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainHeat_LargeHeatResist_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainHeat_LargeHeatResist_ProceduralCookingTriggeredActionEffect());
		}
	}
}
