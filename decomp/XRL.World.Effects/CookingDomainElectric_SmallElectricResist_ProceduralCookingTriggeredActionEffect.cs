using System;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_SmallElectricResist_ProceduralCookingTriggeredActionEffect : BasicTriggeredCookingStatEffect
{
	public CookingDomainElectric_SmallElectricResist_ProceduralCookingTriggeredActionEffect()
		: base("ElectricResistance", "40-50", 300)
	{
	}
}
