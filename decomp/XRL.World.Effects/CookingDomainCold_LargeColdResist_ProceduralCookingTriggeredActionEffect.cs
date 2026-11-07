using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainCold_LargeColdResist_ProceduralCookingTriggeredActionEffect()
		: base("ColdResistance", "125-175", 50)
	{
	}
}
