using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainStrength_LargeStrengthBuff_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainStrength_LargeStrengthBuff_ProceduralCookingTriggeredActionEffect()
		: base("Strength", "8", 50)
	{
	}
}
