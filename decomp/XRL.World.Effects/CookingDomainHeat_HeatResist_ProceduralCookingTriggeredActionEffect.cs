using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainHeat_HeatResist_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainHeat_HeatResist_ProceduralCookingTriggeredActionEffect()
		: base("HeatResistance", "40-50", 300)
	{
	}
}
