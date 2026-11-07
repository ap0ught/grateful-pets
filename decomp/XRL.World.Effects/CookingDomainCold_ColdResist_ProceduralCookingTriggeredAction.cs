using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCold_ColdResist_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain 40-50 Cold Resist for 6 hours.";
	}

	public override string GetNotification()
	{
		return "@they become fortified against the cold.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainCold_ColdResist_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainCold_ColdResist_ProceduralCookingTriggeredActionEffect());
		}
	}
}
