using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHeat_LargeHeatResist_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainHeat_LargeHeatResist_ProceduralCookingTriggeredActionEffect()
		: base("HeatResistance", "125-175", 50)
	{
	}
}
