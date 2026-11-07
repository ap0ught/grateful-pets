using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCold_ColdResist_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainCold_ColdResist_ProceduralCookingTriggeredActionEffect()
		: base("ColdResistance", "40-50", 300)
	{
	}
}
