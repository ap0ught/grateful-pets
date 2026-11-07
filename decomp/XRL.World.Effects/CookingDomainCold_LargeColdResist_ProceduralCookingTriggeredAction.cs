using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain 125-175 cold resist for 50 turns.";
	}

	public override string GetNotification()
	{
		return "@they become impervious to cold.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredActionEffect());
		}
	}
}
