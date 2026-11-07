using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainStrength_LargeStrengthBuff_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "@they gain@s +8 Strength for 50 turns.";
	}

	public override string GetNotification()
	{
		return "@their muscles bulge.";
	}

	public override void Apply(GameObject go)
	{
		if (!go.HasEffect<CookingDomainStrength_LargeStrengthBuff_ProceduralCookingTriggeredActionEffect>())
		{
			go.ApplyEffect(new CookingDomainStrength_LargeStrengthBuff_ProceduralCookingTriggeredActionEffect());
		}
	}
}
